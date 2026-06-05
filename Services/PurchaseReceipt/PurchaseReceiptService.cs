using BackEnd.Constants.Errors;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using BackEnd.DTOs.Mappings;
using BackEnd.DTOs.Requests.Entry;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Requests.PurchaseReceipt;
using BackEnd.DTOs.Responses.Bill;
using BackEnd.DTOs.Responses.PurchaseReceipt;
using BackEnd.Infrastructure.Context;
using BackEnd.Models;
using BackEnd.Utils;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services;

public class PurchaseReceiptService(
    AppDbContext context,
    StockService stockService,
    BillService billService,
    PaymentOrderService paymentOrderService,
    EntryService entryService,
    IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly StockService _stockService = stockService;
    private readonly BillService _billService = billService;
    private readonly PaymentOrderService _paymentOrderService = paymentOrderService;
    private readonly EntryService _entryService = entryService;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<BillWrapperDto>> ReceivePurchaseOrderAsync(CreatePurchaseReceiptDto request)
    {
        if (request.Details == null || request.Details.Count == 0)
            return Result<BillWrapperDto>.Failure(PurchaseReceiptError.DetailsRequired, ErrorType.Validation);

        var purchaseOrderForSupplier = await _context.PurchaseOrdersForSupplier
            .Include(pos => pos.PurchaseOrderDetails)
            .Include(pos => pos.PurchaseOrder)
                .ThenInclude(po => po.PurchaseRequest)
            .Include(pos => pos.Supplier)
            .FirstOrDefaultAsync(pos => pos.Id == request.PurchaseOrderForSupplierId);

        if (purchaseOrderForSupplier == null)
            return Result<BillWrapperDto>.Failure(PurchaseReceiptError.PurchaseOrderNotFound, ErrorType.NotFound);

        if (purchaseOrderForSupplier.SupplierId != request.SupplierId)
            return Result<BillWrapperDto>.Failure(PurchaseReceiptError.SupplierMismatch, ErrorType.Validation);

        var paymentConfirmationResult = await _paymentOrderService.IsPaymentConfirmedAsync(request.PurchaseOrderForSupplierId);
        if (!paymentConfirmationResult.IsSuccess)
            return Result<BillWrapperDto>.Failure(paymentConfirmationResult.ErrorMessage!, paymentConfirmationResult.ErrorType);

        // if (!paymentConfirmationResult.Value)
        //     return Result<BillWrapperDto>.Failure(PurchaseReceiptError.PaymentNotConfirmed, ErrorType.Validation);

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            decimal total = 0;
            decimal taxTotal = 0;

            // Procesar cada producto recibido
            foreach (var detail in request.Details)
            {
                var poDetail = purchaseOrderForSupplier.PurchaseOrderDetails.FirstOrDefault(d => d.ProductId == detail.ProductId);

                if (poDetail == null)
                {
                    await transaction.RollbackAsync();
                    return Result<BillWrapperDto>.Failure($"{PurchaseReceiptError.PurchaseOrderDetailNotFound} (Producto ID: {detail.ProductId})", ErrorType.Validation);
                }

                decimal pendingQuantity = poDetail.QuantityOrdered - poDetail.QuantityReceived;

                if (detail.Quantity > pendingQuantity)
                {
                    await transaction.RollbackAsync();
                    return Result<BillWrapperDto>.Failure($"{PurchaseReceiptError.QuantityExceeded} (Producto ID: {detail.ProductId}, Pendiente: {pendingQuantity}, Intentando recibir: {detail.Quantity})", ErrorType.Validation);
                }

                // 1. Actualizar orden de compra (cantidades recibidas)
                poDetail.QuantityReceived = poDetail.QuantityReceived + detail.Quantity;
                _context.PurchaseOrderDetails.Update(poDetail);

                // 2. Aumentar Stock
                var stockResult = await _stockService.IncreaseStockAsync(detail.ProductId, request.BranchId, detail.Quantity);
                if (!stockResult.IsSuccess)
                {
                    await transaction.RollbackAsync();
                    return Result<BillWrapperDto>.Failure(stockResult.ErrorMessage!, ErrorType.Unexpected);
                }

                // Calcular totales para la factura
                decimal lineTotal = detail.Quantity * detail.Price;
                decimal lineTax = lineTotal * (poDetail.TaxRate / 100m);
                total += lineTotal + lineTax;
                taxTotal += lineTax;
            }

            var allReceived = purchaseOrderForSupplier.PurchaseOrderDetails.All(d => d.QuantityReceived >= d.QuantityOrdered);
            purchaseOrderForSupplier.State = allReceived
                ? PurchaseOrderForSupplierStateEnum.Received
                : PurchaseOrderForSupplierStateEnum.PartiallyReceived;

            _context.PurchaseOrdersForSupplier.Update(purchaseOrderForSupplier);

            await _context.SaveChangesAsync();

            // 3. Crear Factura (Bill) de tipo Compra
            var bill = new Bill
            {
                BillType = BillTypeEnum.CONTADO,
                BillState = BillStateEnum.Pending,
                CustomerId = null,
                PurchaseOrderForSupplierId = purchaseOrderForSupplier.Id,
                Number = request.BillNumber,
                Stamp = request.Stamp,
                Date = request.Date,
                Total = total,
                TaxTotal = taxTotal,
                IsCredit = false
            };

            _context.Bills.Add(bill);
            await _context.SaveChangesAsync();

            // 3.5 Crear Recepción de Compra (PurchaseReceipt)
            var purchaseReceipt = new PurchaseReceipt
            {
                PurchaseOrderForSupplierId = purchaseOrderForSupplier.Id,
                BillId = bill.Id,
                BranchId = request.BranchId,
                SupplierId = request.SupplierId,
                Number = request.BillNumber,
                Stamp = request.Stamp,
                Date = request.Date.ToDateTime(TimeOnly.MinValue),
                Observation = request.Observation,
                Total = total,
                TaxTotal = taxTotal
            };

            _context.PurchaseReceipts.Add(purchaseReceipt);
            await _context.SaveChangesAsync();

            // 4. Crear Detalles de la Factura y Recepción
            foreach (var detail in request.Details)
            {
                var poDetail = purchaseOrderForSupplier.PurchaseOrderDetails.First(d => d.ProductId == detail.ProductId);

                var billDetail = new BillDetail
                {
                    BillId = bill.Id,
                    ProductId = detail.ProductId,
                    Quantity = detail.Quantity,
                    Price = detail.Price,
                    TaxRate = poDetail.TaxRate
                };
                _context.BillDetails.Add(billDetail);

                var receiptDetail = new PurchaseReceiptDetail
                {
                    PurchaseReceiptId = purchaseReceipt.Id,
                    ProductId = detail.ProductId,
                    Quantity = detail.Quantity,
                    Price = detail.Price,
                    TaxRate = poDetail.TaxRate
                };
                _context.PurchaseReceiptDetails.Add(receiptDetail);
            }

            await _context.SaveChangesAsync();

            // 5. Generar asiento contable automático de compra
            var entryDate = request.Date.ToDateTime(TimeOnly.MinValue);
            var activeProcess = await _context.AccountantProcesses
                .FirstOrDefaultAsync(ap => !ap.IsClosed && ap.StartDate <= DateOnly.FromDateTime(entryDate) && ap.EndDate >= DateOnly.FromDateTime(entryDate));

            if (activeProcess == null)
            {
                await transaction.RollbackAsync();
                return Result<BillWrapperDto>.Failure($"No existe un período contable activo para la fecha {request.Date}.", ErrorType.Validation);
            }

            var accountCompras = await _context.AccountPlans
                .FirstOrDefaultAsync(a => a.AccountantProcessId == activeProcess.Id && a.Name.Contains("Compras"));

            var accountIva = await _context.AccountPlans
                .FirstOrDefaultAsync(a => a.AccountantProcessId == activeProcess.Id && (a.Name.Contains("IVA") || a.Name.Contains("Credito Fiscal")));

            var accountCaja = await _context.AccountPlans
                .FirstOrDefaultAsync(a => a.AccountantProcessId == activeProcess.Id && (a.Name.Contains("Caja") || a.Name.Contains("Banco")));

            if (accountCompras == null)
            {
                await transaction.RollbackAsync();
                return Result<BillWrapperDto>.Failure("No se encontró la cuenta contable 'Compras'.", ErrorType.Validation);
            }

            if (accountCaja == null)
            {
                await transaction.RollbackAsync();
                return Result<BillWrapperDto>.Failure("No se encontró la cuenta contable 'Caja/Banco'.", ErrorType.Validation);
            }

            var entryDetails = new List<CreateEntryDetailDto>
            {
                new() { AccountPlanId = accountCompras.Id, Debit = total - taxTotal, Credit = 0m },
                new() { AccountPlanId = accountIva?.Id ?? accountCompras.Id, Debit = taxTotal, Credit = 0m },
                new() { AccountPlanId = accountCaja.Id, Debit = 0m, Credit = total }
            };

            var entryResult = await _entryService.CreateAutomaticEntryAsync(
                entryDate,
                $"Compra: {purchaseOrderForSupplier.Supplier.BusinessName} - Factura N° {request.BillNumber}",
                ModuleEnum.Purchases,
                entryDetails
            );

            if (!entryResult.IsSuccess)
            {
                await transaction.RollbackAsync();
                return Result<BillWrapperDto>.Failure(entryResult.ErrorMessage!, ErrorType.Failure);
            }

            await transaction.CommitAsync();

            return await _billService.GetByIdAsync(bill.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Result<BillWrapperDto>.Failure($"{PurchaseReceiptError.ProcessFailed}: {ex.Message}", ErrorType.Unexpected);
        }
    }

    public async Task<Result<ListBillsWrapperDto>> GetAllAsync()
    {
        var bills = await _context.Bills
            .AsNoTracking()
            .Where(b => b.PurchaseOrderForSupplierId != null)
            .OrderByDescending(b => b.Id)
            .ProjectTo<BillResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Result<ListBillsWrapperDto>.Success(new ListBillsWrapperDto { Bills = bills });
    }

    public async Task<Result<PurchaseReceiptWrapperDto>> GetReceiptByIdAsync(int id)
    {
        var receipt = await LoadQuery()
            .FirstOrDefaultAsync(r => r.Id == id);

        if (receipt == null)
            return Result<PurchaseReceiptWrapperDto>.Failure(PurchaseReceiptError.ReceiptNotFound, ErrorType.NotFound);

        return Result<PurchaseReceiptWrapperDto>.Success(new PurchaseReceiptWrapperDto
        {
            PurchaseReceipt = PurchaseReceiptMapper.MapReceipt(receipt)
        });
    }

    public async Task<Result<ListPurchaseReceiptsWrapperDto>> GetReceiptsAsync(PurchaseReceiptQueryDto queryDto)
    {
        var query = LoadQuery();

        if (queryDto.PurchaseOrderForSupplierId.HasValue)
            query = query.Where(r => r.PurchaseOrderForSupplierId == queryDto.PurchaseOrderForSupplierId.Value);

        if (queryDto.BranchId.HasValue)
            query = query.Where(r => r.BranchId == queryDto.BranchId.Value);

        if (queryDto.SupplierId.HasValue)
            query = query.Where(r => r.SupplierId == queryDto.SupplierId.Value);

        if (queryDto.Date.HasValue)
            query = query.Where(r => r.Date.Date == queryDto.Date.Value.ToDateTime(TimeOnly.MinValue).Date);

        if (queryDto.StartDate.HasValue)
            query = query.Where(r => r.Date >= queryDto.StartDate.Value.ToDateTime(TimeOnly.MinValue));

        if (queryDto.EndDate.HasValue)
            query = query.Where(r => r.Date <= queryDto.EndDate.Value.ToDateTime(TimeOnly.MinValue).AddDays(1));

        var totalElements = await query.CountAsync();

        var receipts = await query
            .OrderByDescending(r => r.Id)
            .Skip((queryDto.Page - 1) * queryDto.PageSize)
            .Take(queryDto.PageSize)
            .ToListAsync();

        return Result<ListPurchaseReceiptsWrapperDto>.Success(new ListPurchaseReceiptsWrapperDto
        {
            PurchaseReceipts = receipts.Select(PurchaseReceiptMapper.MapReceipt).ToList(),
            Pagination = new Pagination(queryDto.Page, queryDto.PageSize, totalElements)
        });
    }

    private IQueryable<PurchaseReceipt> LoadQuery()
    {
        return _context.PurchaseReceipts
            .AsNoTracking()
            .Include(r => r.Branch)
            .Include(r => r.Supplier)
            .Include(r => r.Bill)
            .Include(r => r.PurchaseReceiptDetails)
                .ThenInclude(d => d.Product);
    }
}

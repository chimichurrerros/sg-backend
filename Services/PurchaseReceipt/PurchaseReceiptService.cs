using BackEnd.Constants.Errors;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using BackEnd.DTOs.Requests.PurchaseReceipt;
using BackEnd.DTOs.Responses.Bill;
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
    IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly StockService _stockService = stockService;
    private readonly BillService _billService = billService;
    private readonly PaymentOrderService _paymentOrderService = paymentOrderService;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<BillWrapperDto>> ReceivePurchaseOrderAsync(CreatePurchaseReceiptDto request)
    {
        if (request.Details == null || request.Details.Count == 0)
            return Result<BillWrapperDto>.Failure(PurchaseReceiptError.DetailsRequired, ErrorType.Validation);

        var purchaseOrder = await _context.PurchaseOrders
            .Include(po => po.PurchaseOrderDetails)
            .FirstOrDefaultAsync(po => po.Id == request.PurchaseOrderId);

        if (purchaseOrder == null)
            return Result<BillWrapperDto>.Failure(PurchaseReceiptError.PurchaseOrderNotFound, ErrorType.NotFound);

        var paymentConfirmationResult = await _paymentOrderService.IsPaymentConfirmedAsync(request.PurchaseOrderId);
        if (!paymentConfirmationResult.IsSuccess)
            return Result<BillWrapperDto>.Failure(paymentConfirmationResult.ErrorMessage!, paymentConfirmationResult.ErrorType);

        if (!paymentConfirmationResult.Value)
            return Result<BillWrapperDto>.Failure(PurchaseReceiptError.PaymentNotConfirmed, ErrorType.Validation);

        using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            decimal total = 0;
            decimal taxTotal = 0;

            // Procesar cada producto recibido
            foreach (var detail in request.Details)
            {
                var poDetail = purchaseOrder.PurchaseOrderDetails.FirstOrDefault(d => d.ProductId == detail.ProductId);

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

            var allReceived = purchaseOrder.PurchaseOrderDetails.All(d => d.QuantityReceived >= d.QuantityOrdered);
            purchaseOrder.State = allReceived
                ? PurchaseOrder.PurchaseOrderStateEnum.Received
                : PurchaseOrder.PurchaseOrderStateEnum.PartiallyReceived;

            _context.PurchaseOrders.Update(purchaseOrder);

            await _context.SaveChangesAsync();

            // 3. Crear Factura (Bill) de tipo Compra
            var bill = new Bill
            {
                BillType = BillTypeEnum.CONTADO, // Se podría hacer parametrizable
                BillState = BillStateEnum.Pending,
                CustomerId = null, // Es factura de compra, no hay cliente asociado
                PurchaseOrderId = purchaseOrder.Id,
                Number = request.BillNumber,
                Stamp = request.Stamp,
                Date = request.Date,
                Total = total,
                TaxTotal = taxTotal,
                IsCredit = false
            };

            _context.Bills.Add(bill);
            await _context.SaveChangesAsync();

            // 4. Crear Detalles de la Factura
            foreach (var detail in request.Details)
            {
                var poDetail = purchaseOrder.PurchaseOrderDetails.First(d => d.ProductId == detail.ProductId);

                var billDetail = new BillDetail
                {
                    BillId = bill.Id,
                    ProductId = detail.ProductId,
                    Quantity = detail.Quantity,
                    Price = detail.Price,
                    TaxRate = poDetail.TaxRate
                };
                _context.BillDetails.Add(billDetail);
            }

            await _context.SaveChangesAsync();
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
            .Where(b => b.PurchaseOrderId != null)
            .OrderByDescending(b => b.Id)
            .ProjectTo<BillResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Result<ListBillsWrapperDto>.Success(new ListBillsWrapperDto { Bills = bills });
    }
}

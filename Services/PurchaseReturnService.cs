using BackEnd.Constants.Errors;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Requests.PurchaseReturn;
using BackEnd.DTOs.Responses.PurchaseReturn;
using BackEnd.Infrastructure.Context;
using BackEnd.Models;
using BackEnd.Utils;
using Npgsql;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services;

public class PurchaseReturnService(AppDbContext context, StockService stockService)
{
    private readonly AppDbContext _context = context;
    private readonly StockService _stockService = stockService;

    public async Task<Result<ListPurchaseReturnReasonsWrapperDto>> GetReasonsAsync()
    {
        var reasons = await _context.PurchaseReturnReasons
            .AsNoTracking()
            .Where(reason => reason.IsActive)
            .OrderBy(reason => reason.Name)
            .Select(r => MapReason(r))
            .ToListAsync();

        return Result<ListPurchaseReturnReasonsWrapperDto>.Success(new ListPurchaseReturnReasonsWrapperDto
        {
            Reasons = reasons
        });
    }

    public async Task<Result<PurchaseReturnReasonWrapperDto>> CreateReasonAsync(CreatePurchaseReturnReasonDto request)
    {
        var name = request.Name?.Trim();

        if (string.IsNullOrWhiteSpace(name))
            return Result<PurchaseReturnReasonWrapperDto>.Failure(PurchaseReturnError.ReasonRequired, ErrorType.Validation);

        var existingReason = await _context.PurchaseReturnReasons
            .FirstOrDefaultAsync(reason => reason.Name.ToLower() == name.ToLower());

        if (existingReason != null)
        {
            if (!existingReason.IsActive)
            {
                existingReason.IsActive = true;
                _context.PurchaseReturnReasons.Update(existingReason);
                await _context.SaveChangesAsync();
            }

            return Result<PurchaseReturnReasonWrapperDto>.Success(new PurchaseReturnReasonWrapperDto
            {
                Reason = MapReason(existingReason)
            });
        }

        var reason = new PurchaseReturnReason
        {
            Name = name,
            IsActive = true
        };

        _context.PurchaseReturnReasons.Add(reason);
        await _context.SaveChangesAsync();

        return Result<PurchaseReturnReasonWrapperDto>.Success(new PurchaseReturnReasonWrapperDto
        {
            Reason = MapReason(reason)
        });
    }

    public async Task<Result<ListPurchaseReturnsWrapperDto>> GetListAsync(PurchaseReturnQueryDto queryDto)
    {
        var query = LoadQuery();

        if (!string.IsNullOrWhiteSpace(queryDto.Number))
        {
            query = query.Where(pr => pr.Number.ToLower().Contains(queryDto.Number.ToLower()));
        }

        if (queryDto.Date.HasValue)
        {
            query = query.Where(pr => pr.Date.Date == queryDto.Date.Value.Date);
        }

        if (queryDto.ReasonId.HasValue)
        {
            query = query.Where(pr => pr.ReasonId == queryDto.ReasonId.Value);
        }

        if (!string.IsNullOrWhiteSpace(queryDto.ReasonName))
        {
            query = query.Where(pr => pr.Reason != null && pr.Reason.Name.ToLower().Contains(queryDto.ReasonName.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(queryDto.CustomerName))
        {
            query = query.Where(pr =>
                (pr.Bill != null && pr.Bill.Customer != null && pr.Bill.Customer.Name.ToLower().Contains(queryDto.CustomerName.ToLower())) ||
                (pr.PurchaseOrderForSupplier != null && pr.PurchaseOrderForSupplier.Supplier != null && pr.PurchaseOrderForSupplier.Supplier.BusinessName.ToLower().Contains(queryDto.CustomerName.ToLower()))
            );
        }

        if (!string.IsNullOrWhiteSpace(queryDto.SupplierName))
        {
            query = query.Where(pr => pr.PurchaseOrderForSupplier != null && pr.PurchaseOrderForSupplier.Supplier != null && pr.PurchaseOrderForSupplier.Supplier.BusinessName.ToLower().Contains(queryDto.SupplierName.ToLower()));
        }

        var totalElements = await query.CountAsync();

        var purchaseReturns = await query
            .OrderByDescending(purchaseReturn => purchaseReturn.Id)
            .Skip((queryDto.Page - 1) * queryDto.PageSize)
            .Take(queryDto.PageSize)
            .ToListAsync();

        return Result<ListPurchaseReturnsWrapperDto>.Success(new ListPurchaseReturnsWrapperDto
        {
            PurchaseReturns = purchaseReturns.Select(MapReturn).ToList(),
            Pagination = new Pagination(queryDto.Page, queryDto.PageSize, totalElements)
        });
    }

    public async Task<Result<PurchaseReturnWrapperDto>> GetByIdAsync(int id)
    {
        var purchaseReturn = await LoadQuery()
            .FirstOrDefaultAsync(item => item.Id == id);

        if (purchaseReturn == null)
            return Result<PurchaseReturnWrapperDto>.Failure(PurchaseReturnError.NotFound, ErrorType.NotFound);

        return Result<PurchaseReturnWrapperDto>.Success(new PurchaseReturnWrapperDto
        {
            PurchaseReturn = MapReturn(purchaseReturn)
        });
    }

    public async Task<Result<PurchaseReturnWrapperDto>> CreateAsync(CreatePurchaseReturnDto request)
    {
        if (request.Details == null || request.Details.Count == 0)
            return Result<PurchaseReturnWrapperDto>.Failure(PurchaseReturnError.DetailsRequired, ErrorType.Validation);

        if (request.PurchaseOrderForSupplierId <= 0)
            return Result<PurchaseReturnWrapperDto>.Failure(PurchaseReturnError.PurchaseOrderNotFound, ErrorType.Validation);

        if (request.BranchId <= 0)
            return Result<PurchaseReturnWrapperDto>.Failure(PurchaseReturnError.BranchNotFound, ErrorType.Validation);

        // We'll use a serializable transaction and revalidate quantities before commit
        const int maxRetries = 3;
        for (int attempt = 0; attempt < maxRetries; attempt++)
        {
            // Begin serializable transaction to avoid race conditions
            await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

            try
            {
                var purchaseOrder = await _context.PurchaseOrdersForSupplier
                    .Include(po => po.PurchaseOrderDetails)
                    .FirstOrDefaultAsync(po => po.Id == request.PurchaseOrderForSupplierId);

                if (purchaseOrder == null)
                {
                    await transaction.RollbackAsync();
                    return Result<PurchaseReturnWrapperDto>.Failure(PurchaseReturnError.PurchaseOrderNotFound, ErrorType.NotFound);
                }

                var branchExists = await _context.Branches.AnyAsync(branch => branch.Id == request.BranchId);
                if (!branchExists)
                    return Result<PurchaseReturnWrapperDto>.Failure(PurchaseReturnError.BranchNotFound, ErrorType.NotFound);

                var reasonResult = await ResolveReasonAsync(request.ReasonId, request.ReasonName);
                if (!reasonResult.IsSuccess)
                    return Result<PurchaseReturnWrapperDto>.Failure(reasonResult.ErrorMessage!, reasonResult.ErrorType);

                if (request.BillId.HasValue)
                {
                    var billExists = await _context.Bills.AnyAsync(bill => bill.Id == request.BillId.Value && bill.PurchaseOrderForSupplierId == request.PurchaseOrderForSupplierId);
                    if (!billExists)
                        return Result<PurchaseReturnWrapperDto>.Failure(PurchaseReturnError.BillNotFound, ErrorType.NotFound);
                }

                decimal total = 0;
                decimal taxTotal = 0;

                var purchaseReturn = new PurchaseReturn
                {
                    PurchaseOrderForSupplierId = request.PurchaseOrderForSupplierId,
                    BillId = request.BillId,
                    BranchId = request.BranchId,
                    ReasonId = reasonResult.Value!.Id,
                    Number = string.IsNullOrWhiteSpace(request.Number) ? "TEMP" : request.Number.Trim(),
                    Date = request.Date == default ? DateTime.UtcNow : request.Date,
                    Observation = request.Observation,
                    Total = 0,
                    TaxTotal = 0,
                    State = PurchaseReturnStateEnum.Issued
                };

                _context.PurchaseReturns.Add(purchaseReturn);
                await _context.SaveChangesAsync();

                foreach (var detail in request.Details)
                {
                    if (detail.Quantity <= 0)
                    {
                        await transaction.RollbackAsync();
                        return Result<PurchaseReturnWrapperDto>.Failure(PurchaseReturnError.QuantityExceeded, ErrorType.Validation);
                    }

                    var purchaseOrderDetail = purchaseOrder.PurchaseOrderDetails.FirstOrDefault(item => item.ProductId == detail.ProductId);

                    if (purchaseOrderDetail == null)
                    {
                        await transaction.RollbackAsync();
                        return Result<PurchaseReturnWrapperDto>.Failure($"{PurchaseReturnError.PurchaseOrderDetailNotFound} (Producto ID: {detail.ProductId})", ErrorType.Validation);
                    }

                    var availableToReturn = purchaseOrderDetail.QuantityReceived - purchaseOrderDetail.QuantityReturned;

                    if (detail.Quantity > availableToReturn)
                    {
                        await transaction.RollbackAsync();
                        return Result<PurchaseReturnWrapperDto>.Failure($"{PurchaseReturnError.QuantityExceeded} (Producto ID: {detail.ProductId}, Disponible: {availableToReturn}, Intentando devolver: {detail.Quantity})", ErrorType.Validation);
                    }

                    var stockResult = await _stockService.DecreaseStockAsync(detail.ProductId, request.BranchId, detail.Quantity);
                    if (!stockResult.IsSuccess)
                    {
                        await transaction.RollbackAsync();
                        return Result<PurchaseReturnWrapperDto>.Failure(stockResult.ErrorMessage!, stockResult.ErrorType);
                    }

                    purchaseOrderDetail.QuantityReturned += detail.Quantity;
                    _context.PurchaseOrderDetails.Update(purchaseOrderDetail);

                    var lineTotal = detail.Quantity * detail.Price;
                    var lineTax = lineTotal * (purchaseOrderDetail.TaxRate / 100m);

                    total += lineTotal + lineTax;
                    taxTotal += lineTax;

                    _context.PurchaseReturnDetails.Add(new PurchaseReturnDetail
                    {
                        PurchaseReturnId = purchaseReturn.Id,
                        ProductId = detail.ProductId,
                        Quantity = detail.Quantity,
                        Price = detail.Price,
                        TaxRate = purchaseOrderDetail.TaxRate
                    });
                }

                // Re-validate quantities from DB to avoid race conditions
                foreach (var detail in request.Details)
                {
                    var pod = await _context.PurchaseOrderDetails
                        .AsNoTracking()
                        .FirstOrDefaultAsync(d => d.ProductId == detail.ProductId && d.PurchaseOrderForSupplierId == request.PurchaseOrderForSupplierId);

                    if (pod == null)
                    {
                        await transaction.RollbackAsync();
                        return Result<PurchaseReturnWrapperDto>.Failure($"{PurchaseReturnError.PurchaseOrderDetailNotFound} (Producto ID: {detail.ProductId})", ErrorType.Validation);
                    }

                    if (pod.QuantityReturned > pod.QuantityReceived)
                    {
                        await transaction.RollbackAsync();
                        return Result<PurchaseReturnWrapperDto>.Failure($"{PurchaseReturnError.QuantityExceeded} (Producto ID: {detail.ProductId})", ErrorType.Validation);
                    }
                }

                if (purchaseReturn.BillId.HasValue)
                {
                    var creditNote = new CreditNote
                    {
                        BillId = purchaseReturn.BillId.Value,
                        Number = request.CreditNoteNumber,
                        Type = CreditNoteTypeEnum.PurchaseReturn,
                        Date = purchaseReturn.Date,
                        Total = total,
                        Reason = reasonResult.Value!.Name
                    };
                    _context.CreditNotes.Add(creditNote);
                    await _context.SaveChangesAsync();

                    foreach (var detail in request.Details)
                    {
                        _context.CreditNoteDetails.Add(new CreditNoteDetail
                        {
                            CreditNoteId = creditNote.Id,
                            ProductId = detail.ProductId,
                            Quantity = detail.Quantity,
                            Price = detail.Price
                        });
                    }

                    purchaseReturn.CreditNoteId = creditNote.Id;
                }
                purchaseReturn.Total = total;
                purchaseReturn.TaxTotal = taxTotal;
                purchaseReturn.Number = string.IsNullOrWhiteSpace(request.Number) ? $"PR-{purchaseReturn.Id:D6}" : request.Number.Trim();

                _context.PurchaseReturns.Update(purchaseReturn);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await GetByIdAsync(purchaseReturn.Id);
            }
            catch (Npgsql.PostgresException pex) when (pex.SqlState == "40001")
            {
                // Serialization failure, retry
                try { await _context.Database.RollbackTransactionAsync(); } catch { }
                if (attempt == maxRetries - 1)
                    return Result<PurchaseReturnWrapperDto>.Failure($"{PurchaseReturnError.ProcessFailed}: {pex.Message}", ErrorType.Unexpected);
                // small delay before retry
                await Task.Delay(100 * (attempt + 1));
                continue;
            }
            catch (Exception ex)
            {
                try { await _context.Database.RollbackTransactionAsync(); } catch { }
                return Result<PurchaseReturnWrapperDto>.Failure($"{PurchaseReturnError.ProcessFailed}: {ex.Message}", ErrorType.Unexpected);
            }
        }

        return Result<PurchaseReturnWrapperDto>.Failure(PurchaseReturnError.ProcessFailed, ErrorType.Unexpected);
    }

    private async Task<Result<PurchaseReturnReason>> ResolveReasonAsync(int? reasonId, string? reasonName)
    {
        if (reasonId.HasValue && reasonId.Value > 0)
        {
            var reason = await _context.PurchaseReturnReasons.FirstOrDefaultAsync(item => item.Id == reasonId.Value);
            if (reason == null)
                return Result<PurchaseReturnReason>.Failure(PurchaseReturnError.ReasonNotFound, ErrorType.NotFound);

            if (!reason.IsActive)
            {
                reason.IsActive = true;
                _context.PurchaseReturnReasons.Update(reason);
                await _context.SaveChangesAsync();
            }

            return Result<PurchaseReturnReason>.Success(reason);
        }

        if (!string.IsNullOrWhiteSpace(reasonName))
        {
            var createdReason = await CreateReasonAsync(new CreatePurchaseReturnReasonDto { Name = reasonName });
            if (!createdReason.IsSuccess)
                return Result<PurchaseReturnReason>.Failure(createdReason.ErrorMessage!, createdReason.ErrorType);

            var reason = await _context.PurchaseReturnReasons.FirstAsync(item => item.Id == createdReason.Value!.Reason.Id);
            return Result<PurchaseReturnReason>.Success(reason);
        }

        return Result<PurchaseReturnReason>.Failure(PurchaseReturnError.ReasonRequired, ErrorType.Validation);
    }

    private IQueryable<PurchaseReturn> LoadQuery()
    {
        return _context.PurchaseReturns
            .AsNoTracking()
            .Include(purchaseReturn => purchaseReturn.Branch)
            .Include(purchaseReturn => purchaseReturn.Bill)
                .ThenInclude(bill => bill!.Customer)
            .Include(purchaseReturn => purchaseReturn.CreditNote)
            .Include(purchaseReturn => purchaseReturn.PurchaseOrderForSupplier)
                .ThenInclude(po => po.Supplier)
            .Include(purchaseReturn => purchaseReturn.Reason)
            .Include(purchaseReturn => purchaseReturn.PurchaseReturnDetails)
                .ThenInclude(detail => detail.Product);
    }

    private static PurchaseReturnReasonResponseDto MapReason(PurchaseReturnReason reason)
    {
        return new PurchaseReturnReasonResponseDto
        {
            Id = reason.Id,
            Name = reason.Name,
            IsActive = reason.IsActive
        };
    }

    private static PurchaseReturnDetailResponseDto MapDetail(PurchaseReturnDetail detail)
    {
        return new PurchaseReturnDetailResponseDto
        {
            Id = detail.Id,
            ProductId = detail.ProductId,
            ProductName = detail.Product?.Name ?? string.Empty,
            Quantity = detail.Quantity,
            Price = detail.Price,
            TaxRate = detail.TaxRate,
            LineTotal = detail.Quantity * detail.Price
        };
    }

    private static PurchaseReturnResponseDto MapReturn(PurchaseReturn purchaseReturn)
    {
        return new PurchaseReturnResponseDto
        {
            Id = purchaseReturn.Id,
            PurchaseOrderForSupplierId = purchaseReturn.PurchaseOrderForSupplierId,
            BillId = purchaseReturn.BillId,
            CreditNoteId = purchaseReturn.CreditNoteId,
            BranchId = purchaseReturn.BranchId,
            BranchName = purchaseReturn.Branch?.Name ?? string.Empty,
            ReasonId = purchaseReturn.ReasonId,
            ReasonName = purchaseReturn.Reason?.Name ?? string.Empty,
            Number = purchaseReturn.Number,
            Date = purchaseReturn.Date,
            Observation = purchaseReturn.Observation,
            Total = purchaseReturn.Total,
            TaxTotal = purchaseReturn.TaxTotal,
            State = purchaseReturn.State,
            SupplierName = purchaseReturn.PurchaseOrderForSupplier?.Supplier?.BusinessName ?? string.Empty,
            CustomerName = purchaseReturn.Bill?.Customer?.Name ?? string.Empty,
            Details = purchaseReturn.PurchaseReturnDetails.Select(MapDetail).ToList()
        };
    }

    public async Task<Result<PurchaseReturnWrapperDto>> CreateWithBillAsync(CreateBillAndReturnDto request)
    {
        if (request == null || request.Bill == null || request.Return == null)
            return Result<PurchaseReturnWrapperDto>.Failure(PurchaseReturnError.DetailsRequired, ErrorType.Validation);

        if (request.Return.PurchaseOrderForSupplierId != request.Bill.PurchaseOrderForSupplierId)
            return Result<PurchaseReturnWrapperDto>.Failure(PurchaseReturnError.PurchaseOrderIdMismatch, ErrorType.Validation);

        // Single transaction for both Bill and PurchaseReturn
        await using var transaction = await _context.Database.BeginTransactionAsync(System.Data.IsolationLevel.Serializable);

        try
        {
            // create bill
            var bill = new Bill
            {
                BillType = BillTypeEnum.CONTADO,
                BillState = BillStateEnum.Pending,
                PurchaseOrderForSupplierId = request.Bill.PurchaseOrderForSupplierId,
                Number = string.IsNullOrWhiteSpace(request.Bill.Number) ? $"B-{DateTime.UtcNow:yyyyMMddHHmmss}" : request.Bill.Number.Trim(),
                Stamp = request.Bill.Notes,
                Date = DateOnly.FromDateTime(request.Bill.Date == default ? DateTime.UtcNow : request.Bill.Date),
                Total = request.Bill.Total,
                TaxTotal = request.Bill.TaxTotal,
                IsCredit = false
            };

            _context.Bills.Add(bill);
            await _context.SaveChangesAsync();

            // create purchase return with bill id assigned
            var retRequest = request.Return;
            retRequest.BillId = bill.Id;

            // validate return request
            if (retRequest.Details == null || retRequest.Details.Count == 0)
            {
                await transaction.RollbackAsync();
                return Result<PurchaseReturnWrapperDto>.Failure(PurchaseReturnError.DetailsRequired, ErrorType.Validation);
            }

            if (retRequest.PurchaseOrderForSupplierId <= 0)
            {
                await transaction.RollbackAsync();
                return Result<PurchaseReturnWrapperDto>.Failure(PurchaseReturnError.PurchaseOrderNotFound, ErrorType.Validation);
            }

            if (retRequest.BranchId <= 0)
            {
                await transaction.RollbackAsync();
                return Result<PurchaseReturnWrapperDto>.Failure(PurchaseReturnError.BranchNotFound, ErrorType.Validation);
            }

            var purchaseOrder = await _context.PurchaseOrdersForSupplier
                .Include(po => po.PurchaseOrderDetails)
                .FirstOrDefaultAsync(po => po.Id == retRequest.PurchaseOrderForSupplierId);

            if (purchaseOrder == null)
            {
                await transaction.RollbackAsync();
                return Result<PurchaseReturnWrapperDto>.Failure(PurchaseReturnError.PurchaseOrderNotFound, ErrorType.NotFound);
            }

            var branchExists2 = await _context.Branches.AnyAsync(branch => branch.Id == retRequest.BranchId);
            if (!branchExists2)
            {
                await transaction.RollbackAsync();
                return Result<PurchaseReturnWrapperDto>.Failure(PurchaseReturnError.BranchNotFound, ErrorType.NotFound);
            }

            var reasonResult2 = await ResolveReasonAsync(retRequest.ReasonId, retRequest.ReasonName);
            if (!reasonResult2.IsSuccess)
            {
                await transaction.RollbackAsync();
                return Result<PurchaseReturnWrapperDto>.Failure(reasonResult2.ErrorMessage!, reasonResult2.ErrorType);
            }

            decimal total = 0;
            decimal taxTotal = 0;

            var purchaseReturn = new PurchaseReturn
            {
                PurchaseOrderForSupplierId = retRequest.PurchaseOrderForSupplierId,
                BillId = retRequest.BillId,
                BranchId = retRequest.BranchId,
                ReasonId = reasonResult2.Value!.Id,
                Number = string.IsNullOrWhiteSpace(retRequest.Number) ? "TEMP" : retRequest.Number.Trim(),
                Date = retRequest.Date == default ? DateTime.UtcNow : retRequest.Date,
                Observation = retRequest.Observation,
                Total = 0,
                TaxTotal = 0,
                State = PurchaseReturnStateEnum.Issued
            };

            _context.PurchaseReturns.Add(purchaseReturn);
            await _context.SaveChangesAsync();

            foreach (var detail in retRequest.Details)
            {
                if (detail.Quantity <= 0)
                {
                    await transaction.RollbackAsync();
                    return Result<PurchaseReturnWrapperDto>.Failure(PurchaseReturnError.QuantityExceeded, ErrorType.Validation);
                }

                var purchaseOrderDetail = purchaseOrder.PurchaseOrderDetails.FirstOrDefault(item => item.ProductId == detail.ProductId);

                if (purchaseOrderDetail == null)
                {
                    await transaction.RollbackAsync();
                    return Result<PurchaseReturnWrapperDto>.Failure($"{PurchaseReturnError.PurchaseOrderDetailNotFound} (Producto ID: {detail.ProductId})", ErrorType.Validation);
                }

                var availableToReturn = purchaseOrderDetail.QuantityReceived - purchaseOrderDetail.QuantityReturned;

                if (detail.Quantity > availableToReturn)
                {
                    await transaction.RollbackAsync();
                    return Result<PurchaseReturnWrapperDto>.Failure($"{PurchaseReturnError.QuantityExceeded} (Producto ID: {detail.ProductId}, Disponible: {availableToReturn}, Intentando devolver: {detail.Quantity})", ErrorType.Validation);
                }

                var stockResult = await _stockService.DecreaseStockAsync(detail.ProductId, retRequest.BranchId, detail.Quantity);
                if (!stockResult.IsSuccess)
                {
                    await transaction.RollbackAsync();
                    return Result<PurchaseReturnWrapperDto>.Failure(stockResult.ErrorMessage!, stockResult.ErrorType);
                }

                purchaseOrderDetail.QuantityReturned += detail.Quantity;
                _context.PurchaseOrderDetails.Update(purchaseOrderDetail);

                var lineTotal = detail.Quantity * detail.Price;
                var lineTax = lineTotal * (purchaseOrderDetail.TaxRate / 100m);

                total += lineTotal + lineTax;
                taxTotal += lineTax;

                _context.PurchaseReturnDetails.Add(new PurchaseReturnDetail
                {
                    PurchaseReturnId = purchaseReturn.Id,
                    ProductId = detail.ProductId,
                    Quantity = detail.Quantity,
                    Price = detail.Price,
                    TaxRate = purchaseOrderDetail.TaxRate
                });
            }

            // Re-validate quantities from DB to avoid race conditions
            foreach (var detail in retRequest.Details)
            {
                var pod = await _context.PurchaseOrderDetails
                    .AsNoTracking()
                    .FirstOrDefaultAsync(d => d.ProductId == detail.ProductId && d.PurchaseOrderForSupplierId == retRequest.PurchaseOrderForSupplierId);

                if (pod == null)
                {
                    await transaction.RollbackAsync();
                    return Result<PurchaseReturnWrapperDto>.Failure($"{PurchaseReturnError.PurchaseOrderDetailNotFound} (Producto ID: {detail.ProductId})", ErrorType.Validation);
                }

                if (pod.QuantityReturned > pod.QuantityReceived)
                {
                    await transaction.RollbackAsync();
                    return Result<PurchaseReturnWrapperDto>.Failure($"{PurchaseReturnError.QuantityExceeded} (Producto ID: {detail.ProductId})", ErrorType.Validation);
                }
            }

            var creditNote = new CreditNote
            {
                BillId = purchaseReturn.BillId!.Value,
                Number = retRequest.CreditNoteNumber,
                Type = CreditNoteTypeEnum.PurchaseReturn,
                Date = purchaseReturn.Date,
                Total = total,
                Reason = reasonResult2.Value!.Name
            };
            _context.CreditNotes.Add(creditNote);
            await _context.SaveChangesAsync();

            foreach (var detail in retRequest.Details)
            {
                _context.CreditNoteDetails.Add(new CreditNoteDetail
                {
                    CreditNoteId = creditNote.Id,
                    ProductId = detail.ProductId,
                    Quantity = detail.Quantity,
                    Price = detail.Price
                });
            }

            purchaseReturn.CreditNoteId = creditNote.Id;
            purchaseReturn.Total = total;
            purchaseReturn.TaxTotal = taxTotal;
            purchaseReturn.Number = string.IsNullOrWhiteSpace(retRequest.Number) ? $"PR-{purchaseReturn.Id:D6}" : retRequest.Number.Trim();

            _context.PurchaseReturns.Update(purchaseReturn);
            // update bill state to paid (business decision)
            bill.BillState = BillStateEnum.Paid;
            _context.Bills.Update(bill);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return await GetByIdAsync(purchaseReturn.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Result<PurchaseReturnWrapperDto>.Failure($"{PurchaseReturnError.ProcessFailed}: {ex.Message}", ErrorType.Unexpected);
        }
    }
}

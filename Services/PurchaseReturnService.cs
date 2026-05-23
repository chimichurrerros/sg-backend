using BackEnd.Constants.Errors;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Requests.PurchaseReturn;
using BackEnd.DTOs.Responses.PurchaseReturn;
using BackEnd.Infrastructure.Context;
using BackEnd.Models;
using BackEnd.Utils;
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

    public async Task<Result<ListPurchaseReturnsWrapperDto>> GetListAsync(PaginationRequestDto pagination)
    {
        var query = LoadQuery();
        var totalElements = await query.CountAsync();

        var purchaseReturns = await query
            .OrderByDescending(purchaseReturn => purchaseReturn.Id)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return Result<ListPurchaseReturnsWrapperDto>.Success(new ListPurchaseReturnsWrapperDto
        {
            PurchaseReturns = purchaseReturns.Select(MapReturn).ToList(),
            Pagination = new Pagination(pagination.Page, pagination.PageSize, totalElements)
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

        if (request.PurchaseOrderId <= 0)
            return Result<PurchaseReturnWrapperDto>.Failure(PurchaseReturnError.PurchaseOrderNotFound, ErrorType.Validation);

        if (request.BranchId <= 0)
            return Result<PurchaseReturnWrapperDto>.Failure(PurchaseReturnError.BranchNotFound, ErrorType.Validation);

        var purchaseOrder = await _context.PurchaseOrders
            .Include(purchaseOrder => purchaseOrder.PurchaseOrderDetails)
            .FirstOrDefaultAsync(purchaseOrder => purchaseOrder.Id == request.PurchaseOrderId);

        if (purchaseOrder == null)
            return Result<PurchaseReturnWrapperDto>.Failure(PurchaseReturnError.PurchaseOrderNotFound, ErrorType.NotFound);

        var branchExists = await _context.Branches.AnyAsync(branch => branch.Id == request.BranchId);
        if (!branchExists)
            return Result<PurchaseReturnWrapperDto>.Failure(PurchaseReturnError.BranchNotFound, ErrorType.NotFound);

        var reasonResult = await ResolveReasonAsync(request.ReasonId, request.ReasonName);
        if (!reasonResult.IsSuccess)
            return Result<PurchaseReturnWrapperDto>.Failure(reasonResult.ErrorMessage!, reasonResult.ErrorType);

        if (request.BillId.HasValue)
        {
            var billExists = await _context.Bills.AnyAsync(bill => bill.Id == request.BillId.Value && bill.PurchaseOrderId == request.PurchaseOrderId);
            if (!billExists)
                return Result<PurchaseReturnWrapperDto>.Failure(PurchaseReturnError.BillNotFound, ErrorType.NotFound);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            decimal total = 0;
            decimal taxTotal = 0;

            var purchaseReturn = new PurchaseReturn
            {
                PurchaseOrderId = request.PurchaseOrderId,
                BillId = request.BillId,
                BranchId = request.BranchId,
                ReasonId = reasonResult.Value!.Id,
                Number = string.IsNullOrWhiteSpace(request.Number) ? "TEMP" : request.Number.Trim(),
                Date = request.Date == default ? DateTime.UtcNow : request.Date,
                Observation = request.Observation,
                Total = 0,
                TaxTotal = 0,
                State = PurchaseReturn.PurchaseReturnStateEnum.Issued
            };

            _context.PurchaseReturns.Add(purchaseReturn);
            await _context.SaveChangesAsync();

            foreach (var detail in request.Details)
            {
                if (detail.Quantity <= 0)
                    return Result<PurchaseReturnWrapperDto>.Failure(PurchaseReturnError.QuantityExceeded, ErrorType.Validation);

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

            purchaseReturn.Total = total;
            purchaseReturn.TaxTotal = taxTotal;
            purchaseReturn.Number = string.IsNullOrWhiteSpace(request.Number) ? $"PR-{purchaseReturn.Id:D6}" : request.Number.Trim();

            _context.PurchaseReturns.Update(purchaseReturn);
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
            .Include(purchaseReturn => purchaseReturn.PurchaseOrder)
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
            PurchaseOrderId = purchaseReturn.PurchaseOrderId,
            BillId = purchaseReturn.BillId,
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
            Details = purchaseReturn.PurchaseReturnDetails.Select(MapDetail).ToList()
        };
    }
}
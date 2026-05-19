using BackEnd.Constants.Errors;
using BackEnd.DTOs.Requests.Bank;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Requests.PaymentOrder;
using BackEnd.DTOs.Responses.PaymentOrder;
using BackEnd.Infrastructure.Context;
using BackEnd.Models;
using BackEnd.Utils;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services;

public class PaymentOrderService(AppDbContext context, BankMovementService bankMovementService)
{
    private readonly AppDbContext _context = context;
    private readonly BankMovementService _bankMovementService = bankMovementService;

    public async Task<Result<PaymentOrderWrapperDto>> CreateAsync(CreatePaymentOrderDto request)
    {
        if (request.PurchaseOrderId <= 0)
            return Result<PaymentOrderWrapperDto>.Failure(PaymentOrderError.PurchaseOrderNotFound, ErrorType.Validation);

        if (request.Amount <= 0)
            return Result<PaymentOrderWrapperDto>.Failure(PaymentOrderError.InvalidAmount, ErrorType.Validation);

        var purchaseOrder = await _context.PurchaseOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(po => po.Id == request.PurchaseOrderId);

        if (purchaseOrder == null)
            return Result<PaymentOrderWrapperDto>.Failure(PaymentOrderError.PurchaseOrderNotFound, ErrorType.NotFound);

        if (purchaseOrder.State != PurchaseOrder.PurchaseOrderStateEnum.Confirmed)
            return Result<PaymentOrderWrapperDto>.Failure(PaymentOrderError.PurchaseOrderMustBeConfirmed, ErrorType.Validation);

        var pendingStateId = await ResolveStateIdAsync(["pending"]);
        if (!pendingStateId.HasValue)
            return Result<PaymentOrderWrapperDto>.Failure(PaymentOrderError.PendingStateNotFound, ErrorType.NotFound);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var paymentOrder = new Models.PaymentOrder
            {
                SupplierId = purchaseOrder.SupplierId,
                Date = request.PaymentDate == default ? DateTime.UtcNow : request.PaymentDate,
                Total = request.Amount,
                StateId = pendingStateId.Value
            };

            _context.PaymentOrders.Add(paymentOrder);
            await _context.SaveChangesAsync();

            // We create the payable bill now to keep a direct relation to PurchaseOrder before receipt.
            var bill = new Bill
            {
                BillType = BillTypeEnum.CONTADO,
                BillState = BillStateEnum.Pending,
                PurchaseOrderId = purchaseOrder.Id,
                Number = $"PO-PAY-{paymentOrder.Id:D6}",
                Stamp = request.Notes,
                Date = DateOnly.FromDateTime(paymentOrder.Date),
                Total = request.Amount,
                TaxTotal = 0,
                IsCredit = false
            };

            _context.Bills.Add(bill);
            await _context.SaveChangesAsync();

            _context.PaymentOrderBills.Add(new PaymentOrderBill
            {
                PaymentOrderId = paymentOrder.Id,
                BillId = bill.Id,
                Amount = request.Amount
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return await GetByIdAsync(paymentOrder.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Result<PaymentOrderWrapperDto>.Failure($"{PaymentOrderError.ProcessFailed}: {ex.Message}", ErrorType.Unexpected);
        }
    }

    public async Task<Result<PaymentOrderWrapperDto>> GetByIdAsync(int id)
    {
        var paymentOrder = await LoadQuery()
            .FirstOrDefaultAsync(po => po.Id == id);

        if (paymentOrder == null)
            return Result<PaymentOrderWrapperDto>.Failure(PaymentOrderError.PaymentOrderNotFound, ErrorType.NotFound);

        return Result<PaymentOrderWrapperDto>.Success(new PaymentOrderWrapperDto
        {
            PaymentOrder = MapResponse(paymentOrder)
        });
    }

    public async Task<Result<ListPaymentOrdersWrapperDto>> GetListAsync(PaginationRequestDto pagination)
    {
        var query = LoadQuery();
        var total = await query.CountAsync();

        var paymentOrders = await query
            .OrderByDescending(po => po.Id)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ToListAsync();

        return Result<ListPaymentOrdersWrapperDto>.Success(new ListPaymentOrdersWrapperDto
        {
            PaymentOrders = paymentOrders.Select(MapResponse).ToList(),
            Pagination = new Pagination(pagination.Page, pagination.PageSize, total)
        });
    }

    public async Task<Result<PaymentOrderWrapperDto>> ProcessPaymentAsync(ProcessPaymentOrderDto request)
    {
        if (request.Amount <= 0)
            return Result<PaymentOrderWrapperDto>.Failure(PaymentOrderError.InvalidAmount, ErrorType.Validation);

        var paymentOrder = await _context.PaymentOrders
            .Include(po => po.State)
            .Include(po => po.PaymentOrderBills)
                .ThenInclude(pob => pob.Bill)
            .FirstOrDefaultAsync(po => po.Id == request.PaymentOrderId);

        if (paymentOrder == null)
            return Result<PaymentOrderWrapperDto>.Failure(PaymentOrderError.PaymentOrderNotFound, ErrorType.NotFound);

        if (IsProcessedState(paymentOrder.State?.Name))
            return Result<PaymentOrderWrapperDto>.Failure(PaymentOrderError.PaymentAlreadyProcessed, ErrorType.Validation);

        var accountValidation = await _bankMovementService.ValidateAccountAsync(request.BankAccountId, request.Amount);
        if (!accountValidation.IsSuccess)
            return Result<PaymentOrderWrapperDto>.Failure(accountValidation.ErrorMessage!, accountValidation.ErrorType);

        var processedStateId = await ResolveStateIdAsync(["processed", "paid"]);
        if (!processedStateId.HasValue)
            return Result<PaymentOrderWrapperDto>.Failure(PaymentOrderError.ProcessedStateNotFound, ErrorType.NotFound);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var movementResult = await _bankMovementService.CreateMovementAsync(new CreateBankMovementDto
            {
                AccountId = request.BankAccountId,
                Amount = request.Amount,
                Date = request.PaymentDate == default ? DateTime.UtcNow : request.PaymentDate,
                ReferenceNumber = request.ReferenceNumber,
                MovementType = BankMovementTypeEnum.Debit
            });

            if (!movementResult.IsSuccess)
            {
                await transaction.RollbackAsync();
                return Result<PaymentOrderWrapperDto>.Failure(movementResult.ErrorMessage!, movementResult.ErrorType);
            }

            _context.PaymentOrderMovements.Add(new PaymentOrderMovement
            {
                PaymentOrderId = paymentOrder.Id,
                BankMovementId = movementResult.Value!.Id,
                Amount = request.Amount
            });

            paymentOrder.StateId = processedStateId.Value;
            _context.PaymentOrders.Update(paymentOrder);

            foreach (var billLink in paymentOrder.PaymentOrderBills)
            {
                billLink.Bill.BillState = BillStateEnum.Paid;
                _context.Bills.Update(billLink.Bill);
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return await GetByIdAsync(paymentOrder.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Result<PaymentOrderWrapperDto>.Failure($"{PaymentOrderError.ProcessFailed}: {ex.Message}", ErrorType.Unexpected);
        }
    }

    public async Task<Result<bool>> IsPaymentConfirmedAsync(int purchaseOrderId)
    {
        if (purchaseOrderId <= 0)
            return Result<bool>.Failure(PaymentOrderError.PurchaseOrderNotFound, ErrorType.Validation);

        var isConfirmed = await _context.PaymentOrders
            .AsNoTracking()
            .Include(po => po.State)
            .Include(po => po.PaymentOrderBills)
                .ThenInclude(pob => pob.Bill)
            .AnyAsync(po =>
                po.PaymentOrderBills.Any(pob => pob.Bill.PurchaseOrderId == purchaseOrderId)
                && IsProcessedState(po.State.Name));

        return Result<bool>.Success(isConfirmed);
    }

    private IQueryable<Models.PaymentOrder> LoadQuery()
    {
        return _context.PaymentOrders
            .AsNoTracking()
            .Include(po => po.State)
            .Include(po => po.PaymentOrderBills)
                .ThenInclude(pob => pob.Bill)
            .Include(po => po.PaymentOrderMovements)
                .ThenInclude(pom => pom.BankMovement);
    }

    private static bool IsProcessedState(string? stateName)
    {
        return string.Equals(stateName, "Processed", StringComparison.OrdinalIgnoreCase)
            || string.Equals(stateName, "Paid", StringComparison.OrdinalIgnoreCase);
    }

    private async Task<int?> ResolveStateIdAsync(string[] names)
    {
        var normalized = names.Select(n => n.ToLowerInvariant()).ToList();
        var state = await _context.States
            .AsNoTracking()
            .FirstOrDefaultAsync(s => normalized.Contains(s.Name.ToLower()));

        return state?.Id;
    }

    private static PaymentOrderResponseDto MapResponse(Models.PaymentOrder paymentOrder)
    {
        var purchaseOrderId = paymentOrder.PaymentOrderBills
            .Select(link => link.Bill.PurchaseOrderId)
            .FirstOrDefault(id => id.HasValue) ?? 0;

        return new PaymentOrderResponseDto
        {
            Id = paymentOrder.Id,
            SupplierId = paymentOrder.SupplierId,
            PurchaseOrderId = purchaseOrderId,
            Date = paymentOrder.Date,
            Total = paymentOrder.Total,
            StateId = paymentOrder.State?.Name ?? paymentOrder.StateId.ToString(),
            Bills = paymentOrder.PaymentOrderBills.Select(link => new PaymentOrderBillDto
            {
                Id = link.Id,
                BillId = link.BillId,
                PurchaseOrderId = link.Bill.PurchaseOrderId ?? 0,
                Amount = link.Amount,
                BillNumber = link.Bill.Number
            }).ToList(),
            Movements = paymentOrder.PaymentOrderMovements.Select(movement => new PaymentOrderMovementDto
            {
                Id = movement.Id,
                BankMovementId = movement.BankMovementId,
                BankAccountId = movement.BankMovement.AccountId,
                Amount = movement.Amount,
                Date = movement.BankMovement.Date,
                PaymentMethod = movement.BankMovement.MovementType.ToString(),
                ReferenceNumber = movement.BankMovement.ReferenceNumber
            }).ToList()
        };
    }
}

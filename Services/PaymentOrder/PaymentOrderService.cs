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

        if (request.BankAccountId <= 0)
            return Result<PaymentOrderWrapperDto>.Failure("Invalid bank account", ErrorType.Validation);

        var purchaseOrder = await _context.PurchaseOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(po => po.Id == request.PurchaseOrderId);

        if (purchaseOrder == null)
            return Result<PaymentOrderWrapperDto>.Failure(PaymentOrderError.PurchaseOrderNotFound, ErrorType.NotFound);

        if (purchaseOrder.State != PurchaseOrder.PurchaseOrderStateEnum.Confirmed)
            return Result<PaymentOrderWrapperDto>.Failure(PaymentOrderError.PurchaseOrderMustBeConfirmed, ErrorType.Validation);

        // Validate bank account and funds
        var accountValidation = await _bankMovementService.ValidateAccountAsync(request.BankAccountId, request.Amount);
        if (!accountValidation.IsSuccess)
            return Result<PaymentOrderWrapperDto>.Failure(accountValidation.ErrorMessage!, accountValidation.ErrorType);

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var paymentOrder = new Models.PaymentOrder
            {
                SupplierId = purchaseOrder.SupplierId,
                Date = request.PaymentDate == default ? DateTime.UtcNow : request.PaymentDate,
                Total = request.Amount,
                State = PaymentOrderStateEnum.Processed
            };

            _context.PaymentOrders.Add(paymentOrder);
            await _context.SaveChangesAsync();

            // We create the payable bill now to keep a direct relation to PurchaseOrder before receipt.
            var bill = new Bill
            {
                BillType = BillTypeEnum.CONTADO,
                BillState = BillStateEnum.Paid,
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

            // Create bank movement
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
            .Include(po => po.PaymentOrderBills)
                .ThenInclude(pob => pob.Bill)
            .FirstOrDefaultAsync(po => po.Id == request.PaymentOrderId);

        if (paymentOrder == null)
            return Result<PaymentOrderWrapperDto>.Failure(PaymentOrderError.PaymentOrderNotFound, ErrorType.NotFound);

        if (IsProcessedState(paymentOrder.State))
            return Result<PaymentOrderWrapperDto>.Failure(PaymentOrderError.PaymentAlreadyProcessed, ErrorType.Validation);

        var accountValidation = await _bankMovementService.ValidateAccountAsync(request.BankAccountId, request.Amount);
        if (!accountValidation.IsSuccess)
            return Result<PaymentOrderWrapperDto>.Failure(accountValidation.ErrorMessage!, accountValidation.ErrorType);

        // set enum processed state

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {

            // Apply credit notes first (if any)
            decimal totalCreditApplied = 0m;
            if (request.CreditNotes != null && request.CreditNotes.Any())
            {
                foreach (var cn in request.CreditNotes)
                {
                    if (cn.Amount <= 0)
                    {
                        await transaction.RollbackAsync();
                        return Result<PaymentOrderWrapperDto>.Failure(PaymentOrderError.InvalidAmount, ErrorType.Validation);
                    }

                    var creditNote = await _context.CreditNotes
                        .Include(c => c.CreditNoteDetails)
                        .FirstOrDefaultAsync(c => c.Id == cn.CreditNoteId);

                    if (creditNote == null)
                    {
                        await transaction.RollbackAsync();
                        return Result<PaymentOrderWrapperDto>.Failure($"CreditNote {cn.CreditNoteId} not found", ErrorType.NotFound);
                    }

                    // compute already applied amount
                    var alreadyApplied = await _context.PaymentOrderCreditNotes
                        .Where(pocn => pocn.CreditNoteId == creditNote.Id)
                        .SumAsync(p => (decimal?)p.Amount) ?? 0m;

                    var available = creditNote.Total - alreadyApplied;
                    if (cn.Amount > available)
                    {
                        await transaction.RollbackAsync();
                        return Result<PaymentOrderWrapperDto>.Failure($"CreditNote {cn.CreditNoteId} has insufficient available amount", ErrorType.Validation);
                    }

                    _context.PaymentOrderCreditNotes.Add(new PaymentOrderCreditNote
                    {
                        PaymentOrderId = paymentOrder.Id,
                        CreditNoteId = creditNote.Id,
                        Amount = cn.Amount
                    });

                    totalCreditApplied += cn.Amount;
                }
            }

            // Remaining amount to pay via bank
            var bankAmount = request.Amount - totalCreditApplied;
            if (bankAmount < 0)
            {
                await transaction.RollbackAsync();
                return Result<PaymentOrderWrapperDto>.Failure(PaymentOrderError.InvalidAmount, ErrorType.Validation);
            }

            if (bankAmount > 0)
            {
                var movementResult = await _bankMovementService.CreateMovementAsync(new CreateBankMovementDto
                {
                    AccountId = request.BankAccountId,
                    Amount = bankAmount,
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
                    Amount = bankAmount
                });
            }

            paymentOrder.State = PaymentOrderStateEnum.Processed;
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
            .Include(po => po.PaymentOrderBills)
                .ThenInclude(pob => pob.Bill)
            .AnyAsync(po =>
                po.PaymentOrderBills.Any(pob => pob.Bill.PurchaseOrderId == purchaseOrderId)
                && (po.State == PaymentOrderStateEnum.Processed || po.State == PaymentOrderStateEnum.Paid));

        return Result<bool>.Success(isConfirmed);
    }

    private IQueryable<Models.PaymentOrder> LoadQuery()
    {
        return _context.PaymentOrders
            .AsNoTracking()
            .Include(po => po.PaymentOrderBills)
                .ThenInclude(pob => pob.Bill)
            .Include(po => po.PaymentOrderMovements)
                .ThenInclude(pom => pom.BankMovement)
            .Include(po => po.PaymentOrderCreditNotes)
                .ThenInclude(pocn => pocn.CreditNote);
    }


    private static bool IsProcessedState(PaymentOrderStateEnum state)
    {
        return state == PaymentOrderStateEnum.Processed || state == PaymentOrderStateEnum.Paid;
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
            StateId = paymentOrder.State.ToString(),
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
            ,
            CreditNotes = paymentOrder.PaymentOrderCreditNotes.Select(cn => new PaymentOrderCreditNoteDto
            {
                Id = cn.Id,
                CreditNoteId = cn.CreditNoteId,
                Amount = cn.Amount,
                CreditNoteNumber = cn.CreditNote?.Id.ToString() ?? string.Empty
            }).ToList()
        };
    }
}

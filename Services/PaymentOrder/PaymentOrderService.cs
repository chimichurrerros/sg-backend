using BackEnd.Constants.Errors;
using BackEnd.DTOs.Requests.Bank;
using BackEnd.DTOs.Requests.PaymentOrder;
using BackEnd.DTOs.Responses.PaymentOrder;
using BackEnd.Infrastructure.Context;
using BackEnd.Models;
using BackEnd.Utils;
using Microsoft.EntityFrameworkCore;
using BackEnd.Services.Accounting;
using BackEnd.DTOs.Requests.Entry;

namespace BackEnd.Services;

public class PaymentOrderService(AppDbContext context, BankMovementService bankMovementService, EntryService entryService)
{
    private readonly AppDbContext _context = context;
    private readonly BankMovementService _bankMovementService = bankMovementService;
    private readonly EntryService _entryService = entryService;

    // Crea una orden de pago con multiples metodos
    public async Task<Result<PaymentOrderWrapperDto>> CreateAsync(CreatePaymentOrderDto request)
    {
        if (request.PurchaseOrderForSupplierId <= 0)
            return Result<PaymentOrderWrapperDto>.Failure(PaymentOrderError.PurchaseOrderNotFound, ErrorType.Validation);

        if (request.Methods == null || request.Methods.Count == 0)
            return Result<PaymentOrderWrapperDto>.Failure(PaymentOrderError.DetailsRequired, ErrorType.Validation);

        if (request.Methods.Any(m => m.Amount <= 0))
            return Result<PaymentOrderWrapperDto>.Failure(PaymentOrderError.InvalidAmount, ErrorType.Validation);

        var purchaseOrder = await _context.PurchaseOrdersForSupplier
            .AsNoTracking()
            .FirstOrDefaultAsync(po => po.Id == request.PurchaseOrderForSupplierId);

        if (purchaseOrder == null)
            return Result<PaymentOrderWrapperDto>.Failure(PaymentOrderError.PurchaseOrderNotFound, ErrorType.NotFound);

        if (purchaseOrder.State == PurchaseOrderForSupplierStateEnum.Cancelled)
            return Result<PaymentOrderWrapperDto>.Failure(PaymentOrderError.PurchaseOrderMustNotBeCancelled, ErrorType.Validation);

        var totalAmount = request.Methods.Sum(m => m.Amount);
        if (totalAmount <= 0)
            return Result<PaymentOrderWrapperDto>.Failure(PaymentOrderError.InvalidAmount, ErrorType.Validation);

        var bankMethods = request.Methods
            .Where(m => m.Method != "CreditNote")
            .ToList();

        var creditMethods = request.Methods
            .Where(m => m.Method == "CreditNote")
            .ToList();

        var appliedCreditNotes = new List<(CreditNote CreditNote, decimal Amount)>();

        foreach (var cm in creditMethods)
        {
            if (cm.CreditNoteId == null || cm.CreditNoteId <= 0)
                return Result<PaymentOrderWrapperDto>.Failure(PaymentOrderError.InvalidAmount, ErrorType.Validation);

            var creditNote = await _context.CreditNotes
                .FirstOrDefaultAsync(c => c.Id == cm.CreditNoteId);

            if (creditNote == null)
                return Result<PaymentOrderWrapperDto>.Failure(PaymentOrderError.CreditNoteNotFound, ErrorType.NotFound);

            if (creditNote.Type != CreditNoteTypeEnum.PurchaseReturn)
                return Result<PaymentOrderWrapperDto>.Failure(PaymentOrderError.CreditNoteNotPurchaseReturn, ErrorType.Validation);

            appliedCreditNotes.Add((creditNote, cm.Amount));
        }

        foreach (var bm in bankMethods)
        {
            if (bm.AccountId == null || bm.AccountId <= 0)
                return Result<PaymentOrderWrapperDto>.Failure(PaymentOrderError.BankAccountRequired, ErrorType.Validation);

            var validMethods = new[] { "Transfer", "Check", "Cash" };
            if (!validMethods.Contains(bm.Method))
                return Result<PaymentOrderWrapperDto>.Failure(PaymentOrderError.InvalidPaymentMethod, ErrorType.Validation);

            var accountValidation = await _bankMovementService.ValidateAccountAsync(bm.AccountId.Value, bm.Amount);
            if (!accountValidation.IsSuccess)
                return Result<PaymentOrderWrapperDto>.Failure(accountValidation.ErrorMessage!, accountValidation.ErrorType);
        }

        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            var paymentOrder = new Models.PaymentOrder
            {
                SupplierId = purchaseOrder.SupplierId,
                Date = request.PaymentDate == default ? DateTime.UtcNow : request.PaymentDate,
                Total = totalAmount,
                State = PaymentOrderStateEnum.Processed
            };

            _context.PaymentOrders.Add(paymentOrder);
            await _context.SaveChangesAsync();

            var bill = new Bill
            {
                BillType = BillTypeEnum.CONTADO,
                BillState = BillStateEnum.Paid,
                PurchaseOrderForSupplierId = purchaseOrder.Id,
                Number = $"PO-PAY-{paymentOrder.Id:D6}",
                Stamp = request.Notes,
                Date = DateOnly.FromDateTime(paymentOrder.Date),
                Total = totalAmount,
                TaxTotal = 0,
                IsCredit = false
            };

            _context.Bills.Add(bill);
            await _context.SaveChangesAsync();

            _context.PaymentOrderBills.Add(new PaymentOrderBill
            {
                PaymentOrderId = paymentOrder.Id,
                BillId = bill.Id,
                Amount = totalAmount
            });

            await _context.SaveChangesAsync();

            var debitAccountMap = bill.BillType == BillTypeEnum.CONTADO
                ? AccountantPlanMap.Cajas
                : AccountantPlanMap.Cuentas;

            decimal tenPolcientoTotal = (bill.Total * 10) / 100;
            var entryDetails = new List<CreateEntryDetailDto>
            {
                new CreateEntryDetailDto
                {
                    AccountPlanId = (int)debitAccountMap,
                    Debit = bill.Total,
                    Credit = 0m
                },
                new CreateEntryDetailDto
                {
                    AccountPlanId = (int)AccountantPlanMap.ComprasAProveedores,
                    Debit = 0m,
                    Credit = bill.Total - tenPolcientoTotal
                },
                new CreateEntryDetailDto
                {
                    AccountPlanId = (int)AccountantPlanMap.IVACredito,
                    Debit = 0m,
                    Credit = tenPolcientoTotal
                }
            };

            var entryResult = await _entryService.CreateAutomaticEntryAsync(
                new DateTime(bill.Date.Year, bill.Date.Month, bill.Date.Day, 12, 0, 0, DateTimeKind.Utc),
                $"Factura Recibida Nro. {bill.Number}",
                ModuleEnum.Purchases,
                entryDetails
            );

            if (!entryResult.IsSuccess)
            {
                await transaction.RollbackAsync();
                return Result<PaymentOrderWrapperDto>.Failure($"Error al generar asiento automático: {entryResult.ErrorMessage}", entryResult.ErrorType);
            }

            foreach (var (creditNote, amount) in appliedCreditNotes)
            {
                _context.PaymentOrderCreditNotes.Add(new PaymentOrderCreditNote
                {
                    PaymentOrderId = paymentOrder.Id,
                    CreditNoteId = creditNote.Id,
                    Amount = amount
                });
            }

            if (appliedCreditNotes.Count > 0)
                await _context.SaveChangesAsync();

            foreach (var bm in bankMethods)
            {
                var movementResult = await _bankMovementService.CreateMovementAsync(new CreateBankMovementDto
                {
                    AccountId = bm.AccountId!.Value,
                    Amount = bm.Amount,
                    Date = request.PaymentDate == default ? DateTime.UtcNow : request.PaymentDate,
                    ReferenceNumber = bm.ReferenceNumber,
                    MovementType = BankMovementTypeEnum.Debit,
                    CheckDetails = bm.Method == "Check" ? bm.CheckDetails : null
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
                    Amount = bm.Amount
                });
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return await GetByIdAsync(paymentOrder.Id);
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            var message = ex.InnerException?.Message ?? ex.Message;
            return Result<PaymentOrderWrapperDto>.Failure($"{PaymentOrderError.ProcessFailed}: {message}", ErrorType.Unexpected);
        }
    }

    // Busca una orden de pago por su ID cargando facturas y movimientos
    public async Task<Result<PaymentOrderWrapperDto>> GetByIdAsync(int id)
    {
        var paymentOrder = await LoadQuery()
            .FirstOrDefaultAsync(po => po.Id == id);

        // Si no la encuentra, devuelve error 404
        if (paymentOrder == null)
            return Result<PaymentOrderWrapperDto>.Failure(PaymentOrderError.PaymentOrderNotFound, ErrorType.NotFound);

        // Mapea la entidad manualmente a DTO de respuesta
        return Result<PaymentOrderWrapperDto>.Success(new PaymentOrderWrapperDto
        {
            PaymentOrder = MapResponse(paymentOrder)
        });
    }

    // Obtiene una lista paginada de las órdenes de pago
    public async Task<Result<ListPaymentOrdersWrapperDto>> GetListAsync(PaymentOrderQueryDto query)
    {
        var rQuery = LoadQuery();

        if (query.SupplierId.HasValue)
            rQuery = rQuery.Where(po => po.SupplierId == query.SupplierId.Value);

        if (query.State.HasValue)
            rQuery = rQuery.Where(po => (int)po.State == query.State.Value);

        if (query.PurchaseOrderForSupplierId.HasValue)
            rQuery = rQuery.Where(po => po.PaymentOrderBills.Any(pob => pob.Bill.PurchaseOrderForSupplierId == query.PurchaseOrderForSupplierId.Value));

        if (query.StartDate.HasValue)
            rQuery = rQuery.Where(po => DateOnly.FromDateTime(po.Date) >= query.StartDate.Value);

        if (query.EndDate.HasValue)
            rQuery = rQuery.Where(po => DateOnly.FromDateTime(po.Date) <= query.EndDate.Value);

        var total = await rQuery.CountAsync();

        var paymentOrders = await rQuery
            .OrderByDescending(po => po.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync();

        return Result<ListPaymentOrdersWrapperDto>.Success(new ListPaymentOrdersWrapperDto
        {
            PaymentOrders = paymentOrders.Select(MapResponse).ToList(),
            Pagination = new Pagination(query.Page, query.PageSize, total)
        });
    }

    // Procesa un pago para una orden de pago ya existente que estaba pendiente
    public async Task<Result<PaymentOrderWrapperDto>> ProcessPaymentAsync(ProcessPaymentOrderDto request)
    {
        if (request.Amount <= 0)
            return Result<PaymentOrderWrapperDto>.Failure(PaymentOrderError.InvalidAmount, ErrorType.Validation);

        // Obtiene la orden de pago junto con las facturas vinculadas
        var paymentOrder = await _context.PaymentOrders
            .Include(po => po.PaymentOrderBills)
                .ThenInclude(pob => pob.Bill)
            .FirstOrDefaultAsync(po => po.Id == request.PaymentOrderId);

        // Si no la encuentra, devuelve 404
        if (paymentOrder == null)
            return Result<PaymentOrderWrapperDto>.Failure(PaymentOrderError.PaymentOrderNotFound, ErrorType.NotFound);

        // Valida que no se intente procesar un pago ya procesado o pagado anteriormente
        if (IsProcessedState(paymentOrder.State))
            return Result<PaymentOrderWrapperDto>.Failure(PaymentOrderError.PaymentAlreadyProcessed, ErrorType.Validation);

        // Valida saldo disponible en la cuenta bancaria seleccionada
        var accountValidation = await _bankMovementService.ValidateAccountAsync(request.BankAccountId, request.Amount);
        if (!accountValidation.IsSuccess)
            return Result<PaymentOrderWrapperDto>.Failure(accountValidation.ErrorMessage!, accountValidation.ErrorType);

        // Inicia transacción segura
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // Crea el movimiento en el banco (Débito)
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

            // Vincula el movimiento bancario
            _context.PaymentOrderMovements.Add(new PaymentOrderMovement
            {
                PaymentOrderId = paymentOrder.Id,
                BankMovementId = movementResult.Value!.Id,
                Amount = request.Amount
            });

            // Cambia el estado de la orden de pago a Procesada
            paymentOrder.State = PaymentOrderStateEnum.Processed;
            _context.PaymentOrders.Update(paymentOrder);

            // Marca todas las facturas vinculadas como Pagadas
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
            var message = ex.InnerException?.Message ?? ex.Message;
            return Result<PaymentOrderWrapperDto>.Failure($"{PaymentOrderError.ProcessFailed}: {message}", ErrorType.Unexpected);
        }
    }

    // Verifica si ya se ha confirmado/procesado algún pago para una orden de compra específica
    public async Task<Result<bool>> IsPaymentConfirmedAsync(int purchaseOrderId)
    {
        if (purchaseOrderId <= 0)
            return Result<bool>.Failure(PaymentOrderError.PurchaseOrderNotFound, ErrorType.Validation);

        // Revisa si existe alguna orden de pago procesada/pagada que esté vinculada a esa orden de compra
        var isConfirmed = await _context.PaymentOrders
            .AsNoTracking()
            .Include(po => po.PaymentOrderBills)
                .ThenInclude(pob => pob.Bill)
            .AnyAsync(po =>
                po.PaymentOrderBills.Any(pob => pob.Bill.PurchaseOrderForSupplierId == purchaseOrderId)
                && (po.State == PaymentOrderStateEnum.Processed || po.State == PaymentOrderStateEnum.Paid));

        return Result<bool>.Success(isConfirmed);
    }

    // Consulta base reutilizable para traer órdenes de pago con facturas y movimientos
    private IQueryable<Models.PaymentOrder> LoadQuery()
    {
        return _context.PaymentOrders
            .AsNoTracking()
            .Include(po => po.PaymentOrderBills)
                .ThenInclude(pob => pob.Bill)
            .Include(po => po.PaymentOrderMovements)
                .ThenInclude(pom => pom.BankMovement)
                .ThenInclude(bm => bm.Check)
            .Include(po => po.PaymentOrderCreditNotes)
                .ThenInclude(pocn => pocn.CreditNote);
    }

    // Verifica si un estado de la orden de pago corresponde a Procesado o Pagado
    private static bool IsProcessedState(PaymentOrderStateEnum state)
    {
        return state == PaymentOrderStateEnum.Processed || state == PaymentOrderStateEnum.Paid;
    }

    // Mapeo manual de la entidad a DTO de respuesta para estructurar la respuesta del controlador
    private static PaymentOrderResponseDto MapResponse(Models.PaymentOrder paymentOrder)
    {
        // Intenta obtener el ID de orden de compra desde las facturas asociadas
        var purchaseOrderId = paymentOrder.PaymentOrderBills
            .Select(link => link.Bill.PurchaseOrderForSupplierId)
            .FirstOrDefault(id => id.HasValue) ?? 0;

        var methods = new List<string>();

        var hasCheck = paymentOrder.PaymentOrderMovements
            .Any(m => m.BankMovement.Check != null);
        var hasRegularMovements = paymentOrder.PaymentOrderMovements
            .Any(m => m.BankMovement.Check == null);
        var hasCreditNotes = paymentOrder.PaymentOrderCreditNotes.Any();

        if (hasCheck)
            methods.Add("Check");
        if (hasRegularMovements)
            methods.Add("Transfer");
        if (hasCreditNotes)
            methods.Add("CreditNote");

        var paymentMethod = methods.Count > 0 ? string.Join(", ", methods) : "Bank";

        return new PaymentOrderResponseDto
        {
            Id = paymentOrder.Id,
            SupplierId = paymentOrder.SupplierId,
            PurchaseOrderForSupplierId = purchaseOrderId,
            Date = paymentOrder.Date,
            Total = paymentOrder.Total,
            StateId = paymentOrder.State.ToString(),
            PaymentMethod = paymentMethod,

            // Mapea la lista de facturas relacionadas
            Bills = paymentOrder.PaymentOrderBills.Select(link => new PaymentOrderBillDto
            {
                Id = link.Id,
                BillId = link.BillId,
                PurchaseOrderForSupplierId = link.Bill.PurchaseOrderForSupplierId ?? 0,
                Amount = link.Amount,
                BillNumber = link.Bill.Number
            }).ToList(),

            // Mapea los movimientos bancarios realizados
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

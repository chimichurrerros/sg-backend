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

    // Crea una orden de pago nueva
    public async Task<Result<PaymentOrderWrapperDto>> CreateAsync(CreatePaymentOrderDto request)
    {
        // Validaciones básicas de que los IDs y montos sean lógicos
        if (request.PurchaseOrderId <= 0)
            return Result<PaymentOrderWrapperDto>.Failure(PaymentOrderError.PurchaseOrderNotFound, ErrorType.Validation);

        if (request.Amount <= 0)
            return Result<PaymentOrderWrapperDto>.Failure(PaymentOrderError.InvalidAmount, ErrorType.Validation);

        if (request.BankAccountId <= 0)
            return Result<PaymentOrderWrapperDto>.Failure("Invalid bank account", ErrorType.Validation);

        // Busca la orden de compra asociada
        var purchaseOrder = await _context.PurchaseOrders
            .AsNoTracking()
            .FirstOrDefaultAsync(po => po.Id == request.PurchaseOrderId);

        // Si no existe la orden de compra, devuelve error 404
        if (purchaseOrder == null)
            return Result<PaymentOrderWrapperDto>.Failure(PaymentOrderError.PurchaseOrderNotFound, ErrorType.NotFound);

        // La orden de compra obligatoriamente tiene que estar Confirmada para pagarse
        if (purchaseOrder.State != PurchaseOrder.PurchaseOrderStateEnum.Confirmed)
            return Result<PaymentOrderWrapperDto>.Failure(PaymentOrderError.PurchaseOrderMustBeConfirmed, ErrorType.Validation);

        // Valida que la cuenta bancaria exista y tenga saldo disponible suficiente para el pago
        var accountValidation = await _bankMovementService.ValidateAccountAsync(request.BankAccountId, request.Amount);
        if (!accountValidation.IsSuccess)
            return Result<PaymentOrderWrapperDto>.Failure(accountValidation.ErrorMessage!, accountValidation.ErrorType);

        // Iniciamos transacción para asegurar que se cree la orden, la factura, el movimiento de banco y sus relaciones de forma segura
        await using var transaction = await _context.Database.BeginTransactionAsync();

        try
        {
            // 1. Crea la cabecera de la Orden de Pago (nace como Procesada directamente)
            var paymentOrder = new Models.PaymentOrder
            {
                SupplierId = purchaseOrder.SupplierId,
                Date = request.PaymentDate == default ? DateTime.UtcNow : request.PaymentDate,
                Total = request.Amount,
                State = PaymentOrderStateEnum.Processed
            };

            _context.PaymentOrders.Add(paymentOrder);
            await _context.SaveChangesAsync();

            // 2. Crea la factura (Bill) al contado y pagada vinculada a la Orden de Compra
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

            // 3. Vincula la orden de pago recién creada con la factura
            _context.PaymentOrderBills.Add(new PaymentOrderBill
            {
                PaymentOrderId = paymentOrder.Id,
                BillId = bill.Id,
                Amount = request.Amount
            });

            await _context.SaveChangesAsync();

            // 4. Registra el movimiento bancario (Débito / Salida de dinero)
            var movementResult = await _bankMovementService.CreateMovementAsync(new CreateBankMovementDto
            {
                AccountId = request.BankAccountId,
                Amount = request.Amount,
                Date = request.PaymentDate == default ? DateTime.UtcNow : request.PaymentDate,
                ReferenceNumber = request.ReferenceNumber,
                MovementType = BankMovementTypeEnum.Debit
            });

            // Si falla el registro del movimiento en el banco, cancelamos todo
            if (!movementResult.IsSuccess)
            {
                await transaction.RollbackAsync();
                return Result<PaymentOrderWrapperDto>.Failure(movementResult.ErrorMessage!, movementResult.ErrorType);
            }

            // 5. Vincula el movimiento bancario con la orden de pago
            _context.PaymentOrderMovements.Add(new PaymentOrderMovement
            {
                PaymentOrderId = paymentOrder.Id,
                BankMovementId = movementResult.Value!.Id,
                Amount = request.Amount
            });

            // Guarda los cambios y confirma la transacción en la base de datos
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            // Retorna la orden de pago completa obteniéndola por ID
            return await GetByIdAsync(paymentOrder.Id);
        }
        catch (Exception ex)
        {
            // Ante cualquier error inesperado, deshace todos los registros creados a medias
            await transaction.RollbackAsync();
            return Result<PaymentOrderWrapperDto>.Failure($"{PaymentOrderError.ProcessFailed}: {ex.Message}", ErrorType.Unexpected);
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
    public async Task<Result<ListPaymentOrdersWrapperDto>> GetListAsync(PaginationRequestDto pagination)
    {
        var query = LoadQuery();
        var total = await query.CountAsync(); // Total general para paginación

        // Trae las órdenes correspondientes a la página
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
            return Result<PaymentOrderWrapperDto>.Failure($"{PaymentOrderError.ProcessFailed}: {ex.Message}", ErrorType.Unexpected);
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
                po.PaymentOrderBills.Any(pob => pob.Bill.PurchaseOrderId == purchaseOrderId)
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
                .ThenInclude(pom => pom.BankMovement);
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
            
            // Mapea la lista de facturas relacionadas
            Bills = paymentOrder.PaymentOrderBills.Select(link => new PaymentOrderBillDto
            {
                Id = link.Id,
                BillId = link.BillId,
                PurchaseOrderId = link.Bill.PurchaseOrderId ?? 0,
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
        };
    }
}

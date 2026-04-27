using AutoMapper;
using AutoMapper.QueryableExtensions;
using BackEnd.Constants.Errors;
using BackEnd.DTOs.Requests.SalesOrder;
using BackEnd.DTOs.Responses.SalesOrder;
using BackEnd.Infrastructure.Context;
using BackEnd.Models;
using BackEnd.Utils;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services;

public class SalesOrderService(AppDbContext context, IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<SalesOrderWrapperDto>> CreateAsync(CreateSalesOrderRequestDto request)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Create SalesOrder
            var salesOrder = new SalesOrder
            {
                CustomerId = request.CustomerId,
                UserId = request.UserId,
                Number = request.Number,
                Date = DateTime.UtcNow,
                StateId = request.StateId,
                Total = 0 // Will compute
            };

            _context.SalesOrders.Add(salesOrder);
            await _context.SaveChangesAsync();

            decimal total = 0;
            decimal taxTotal = 0;

            // 2. Process Details
            foreach (var detail in request.Details)
            {
                var lineTotal = detail.Quantity * detail.Price;
                var lineTax = lineTotal * (detail.TaxRate / 100);

                total += lineTotal + lineTax;
                taxTotal += lineTax;

                // Add SalesOrderDetail
                var salesOrderDetail = new SalesOrderDetail
                {
                    SalesOrderId = salesOrder.Id,
                    ProductId = detail.ProductId,
                    QuantityOrdered = detail.Quantity,
                    QuantityInvoiced = detail.Quantity,
                    Price = detail.Price,
                    TaxRate = detail.TaxRate
                };
                _context.SalesOrderDetails.Add(salesOrderDetail);

                // Decrease Stock
                var stock = await _context.Stocks
                    .FirstOrDefaultAsync(s => s.ProductId == detail.ProductId && s.BranchId == request.BranchId);

                if (stock != null)
                {
                    stock.Quantity -= detail.Quantity;
                    _context.Stocks.Update(stock);
                }
                else
                {
                    stock = new Stock
                    {
                        ProductId = detail.ProductId,
                        BranchId = request.BranchId,
                        Quantity = -detail.Quantity
                    };
                    _context.Stocks.Add(stock);
                }
            }

            salesOrder.Total = total;
            _context.SalesOrders.Update(salesOrder);

            // 3. Create Bill
            var bill = new Bill
            {
                BillType = BillTypeEnum.CONTADO,
                EntityId = request.EntityId,
                SalesOrderId = salesOrder.Id,
                Number = request.BillNumber,
                Date = DateOnly.FromDateTime(DateTime.UtcNow),
                Total = total,
                TaxTotal = taxTotal,
                StateId = request.BillStateId,
                IsCredit = false
            };
            _context.Bills.Add(bill);
            await _context.SaveChangesAsync();

            // Create Bill Details
            foreach (var detail in request.Details)
            {
                var billDetail = new BillDetail
                {
                    BillId = bill.Id,
                    ProductId = detail.ProductId,
                    Quantity = detail.Quantity,
                    Price = detail.Price,
                    TaxRate = detail.TaxRate
                };
                _context.BillDetails.Add(billDetail);
            }

            // 4. Increase Account Balance
            var account = await _context.Accounts.FindAsync(request.AccountId);
            if (account == null)
            {
                throw new Exception($"Account with ID {request.AccountId} not found.");
            }

            account.CurrentBalance += total;
            account.AvailableBalance += total;
            _context.Accounts.Update(account);

            // 5. Create Bank Movement
            var bankMovement = new BankMovement
            {
                AccountId = request.AccountId,
                MovementTypeId = request.MovementTypeId,
                Date = DateTime.UtcNow,
                Amount = total,
                ReferenceNumber = $"SALE-{salesOrder.Id}"
            };
            _context.BankMovements.Add(bankMovement);

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            var responseDto = new SalesOrderResponseDto
            {
                Id = salesOrder.Id,
                CustomerId = salesOrder.CustomerId,
                UserId = salesOrder.UserId,
                Number = salesOrder.Number,
                Date = salesOrder.Date,
                Total = salesOrder.Total,
                StateId = salesOrder.StateId
            };

            return Result<SalesOrderWrapperDto>.Success(new SalesOrderWrapperDto { SalesOrder = responseDto });
        }
        catch (Exception ex)
        {
            await transaction.RollbackAsync();
            return Result<SalesOrderWrapperDto>.Failure($"Failed to process sale: {ex.Message}", ErrorType.Validation);
        }
    }
}

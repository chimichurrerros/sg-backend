using AutoMapper;
using AutoMapper.QueryableExtensions;
using BackEnd.Infrastructure.Context;
using BackEnd.Constants.Errors;
using BackEnd.Utils;
using BackEnd.DTOs.Responses.Stock;
using Microsoft.EntityFrameworkCore;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Requests.Stock;
using BackEnd.Models;

namespace BackEnd.Services;

public class StockService(AppDbContext context, IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ListStocksWrapperDto>> GetAllAsync()
    {
        var stocks = await _context.Stocks
            .AsNoTracking()
            .ProjectTo<StockResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Result<ListStocksWrapperDto>.Success(new ListStocksWrapperDto { Stocks = stocks });
    }

    public async Task<Result<ListStocksWrapperDto>> GetListAsync(StockQueryDto query)
    {
        var rQuery = _context.Stocks.AsNoTracking();

        if (query.ProductId.HasValue)
            rQuery = rQuery.Where(s => s.ProductId == query.ProductId.Value);

        if (query.BranchId.HasValue)
            rQuery = rQuery.Where(s => s.BranchId == query.BranchId.Value);

        if (query.MinQuantity.HasValue)
            rQuery = rQuery.Where(s => s.Quantity >= query.MinQuantity.Value);

        if (query.MaxQuantity.HasValue)
            rQuery = rQuery.Where(s => s.Quantity <= query.MaxQuantity.Value);

        if (!string.IsNullOrWhiteSpace(query.Search))
            rQuery = rQuery.Where(s => s.Product.Name.Contains(query.Search));

        if (query.BelowMinimum == true)
            rQuery = rQuery.Where(s => s.Product.MinimumStock != null && s.Quantity < s.Product.MinimumStock);

        var totalElements = await rQuery.CountAsync();

        var stocks = await rQuery
            .OrderBy(s => s.Id)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ProjectTo<StockResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var _pagination = new Pagination(query.Page, query.PageSize, totalElements);

        return Result<ListStocksWrapperDto>.Success(new ListStocksWrapperDto { Stocks = stocks, Pagination = _pagination });
    }

    public async Task<Result<StockWrapperDto>> GetByIdAsync(int id)
    {
        var stock = await _context.Stocks
            .AsNoTracking()
            .Where(u => u.Id == id)
            .ProjectTo<StockResponseDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (stock == null)
            return Result<StockWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        return Result<StockWrapperDto>.Success(new StockWrapperDto { Stock = stock });
    }

    public async Task<Result<StockWrapperDto>> CreateAsync(StockRequestDto request)
    {
        var stock = _mapper.Map<Stock>(request);

        _context.Stocks.Add(stock);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(stock.Id);
    }

    public async Task<Result<StockWrapperDto>> UpdateAsync(int id, StockRequestDto request)
    {
        var stock = await _context.Stocks.FindAsync(id);

        if (stock == null)
            return Result<StockWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        _mapper.Map(request, stock);
        _context.Stocks.Update(stock);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(stock.Id);
    }

    public async Task<Result> DecreaseStockAsync(int productId, int branchId, decimal? quantity)
    {
        if (quantity <= 0)
            return Result.Failure(StockError.QuantityMustBeGreaterThanZero, ErrorType.Validation);

        var stock = await _context.Stocks
            .FirstOrDefaultAsync(s => s.ProductId == productId && s.BranchId == branchId);

        if (stock == null)
        {
            stock = new Stock
            {
                ProductId = productId,
                BranchId = branchId,
                Quantity = quantity != null ? -quantity : null 
            };
            _context.Stocks.Add(stock);
        }
        else
        {
            if (quantity != null)
            {
                stock.Quantity -= quantity;
                _context.Stocks.Update(stock);
            }
        }

        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> ValidateSufficientStockAsync(int productId, int branchId, decimal requiredQuantity)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null)
            return Result.Failure(string.Format(StockError.InsufficientStock, "desconocido", 0, requiredQuantity), ErrorType.Validation);

        var stock = await _context.Stocks
            .FirstOrDefaultAsync(s => s.ProductId == productId && s.BranchId == branchId);

        var availableQuantity = stock?.Quantity ?? 0;

        if (availableQuantity < requiredQuantity)
        {
            var errorMsg = string.Format(StockError.InsufficientStock, product.Name, availableQuantity, requiredQuantity);
            return Result.Failure(errorMsg, ErrorType.Validation);
        }

        return Result.Success();
    }

    public async Task<Result> IncreaseStockAsync(int productId, int branchId, decimal quantity)
    {
        if (quantity <= 0)
            return Result.Failure(StockError.QuantityMustBeGreaterThanZero, ErrorType.Validation);

        var stock = await _context.Stocks
            .FirstOrDefaultAsync(s => s.ProductId == productId && s.BranchId == branchId);

        if (stock == null)
        {
            stock = new Stock
            {
                ProductId = productId,
                BranchId = branchId,
                Quantity = quantity
            };
            _context.Stocks.Add(stock);
        }
        else
        {
            stock.Quantity += quantity;
            _context.Stocks.Update(stock);
        }

        await _context.SaveChangesAsync();
        return Result.Success();
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var stock = await _context.Stocks.FindAsync(id);

        if (stock == null)
            return Result.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        _context.Stocks.Remove(stock);
        await _context.SaveChangesAsync();

        return Result.Success();
    }
}

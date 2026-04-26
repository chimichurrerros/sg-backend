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

    public async Task<Result<ListStocksWrapperDto>> GetListAsync(PaginationRequestDto pagination)
    {
        var query = _context.Stocks.AsNoTracking();
        
        var totalElements = await query.CountAsync();
        
        var stocks = await query
            .OrderBy(v => v.Id)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectTo<StockResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var _pagination = new Pagination(pagination.Page, pagination.PageSize, totalElements);
            
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

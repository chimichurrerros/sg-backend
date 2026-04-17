using AutoMapper;
using AutoMapper.QueryableExtensions;
using BackEnd.Infrastructure.Context;
using BackEnd.Constants.Errors;
using BackEnd.Utils;
using BackEnd.DTOs.Responses.Product;
using Microsoft.EntityFrameworkCore;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Requests.Product;
using BackEnd.Models;

namespace BackEnd.Services;

public class ProductsService(AppDbContext context, IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ListProductsWrapperDto>> GetAllAsync()
    {
        var products = await _context.Products
            .AsNoTracking()
            .ProjectTo<ProductResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Result<ListProductsWrapperDto>.Success(new ListProductsWrapperDto { Products = products });
    }

    public async Task<Result<ListProductsWrapperDto>> GetListAsync(PaginationRequestDto pagination)
    {
        var query = _context.Products.AsNoTracking();
        
        var totalElements = await query.CountAsync();
        
        var products = await query
            .OrderBy(v => v.Id)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectTo<ProductResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var _pagination = new Pagination(pagination.Page, pagination.PageSize, totalElements);
            
        return Result<ListProductsWrapperDto>.Success(new ListProductsWrapperDto { Products = products, Pagination = _pagination });
    }

    public async Task<Result<ProductWrapperDto>> GetByIdAsync(int id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Where(u => u.Id == id)
            .ProjectTo<ProductResponseDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (product == null)
            return Result<ProductWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        return Result<ProductWrapperDto>.Success(new ProductWrapperDto { Product = product });
    }

    public async Task<Result<ProductWrapperDto>> CreateAsync(ProductRequestDto request)
    {
        var product = _mapper.Map<Product>(request);
        
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        
        // Reload to get names for the DTO
        return await GetByIdAsync(product.Id);
    }

    public async Task<Result<ProductWrapperDto>> UpdateAsync(int id, ProductRequestDto request)
    {
        var product = await _context.Products.FindAsync(id);
        
        if (product == null)
            return Result<ProductWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        _mapper.Map(request, product);
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
        
        return await GetByIdAsync(product.Id);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var product = await _context.Products.FindAsync(id);
        
        if (product == null)
            return Result.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        
        return Result.Success();
    }
}
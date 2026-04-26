using AutoMapper;
using AutoMapper.QueryableExtensions;
using BackEnd.Infrastructure.Context;
using BackEnd.Constants.Errors;
using BackEnd.Utils;
using BackEnd.DTOs.Responses.ProductCategory;
using Microsoft.EntityFrameworkCore;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Requests.ProductCategory;
using BackEnd.Models;

namespace BackEnd.Services;

public class ProductCategoriesService(AppDbContext context, IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ListProductCategoriesWrapperDto>> GetAllAsync()
    {
        var categories = await _context.ProductCategories
            .AsNoTracking()
            .ProjectTo<ProductCategoryResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Result<ListProductCategoriesWrapperDto>.Success(new ListProductCategoriesWrapperDto { ProductCategories = categories });
    }

    public async Task<Result<ListProductCategoriesWrapperDto>> GetListAsync(PaginationRequestDto pagination)
    {
        var query = _context.ProductCategories.AsNoTracking();
        
        var totalElements = await query.CountAsync();
        
        var categories = await query
            .OrderBy(v => v.Id)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectTo<ProductCategoryResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var _pagination = new Pagination(pagination.Page, pagination.PageSize, totalElements);
            
        return Result<ListProductCategoriesWrapperDto>.Success(new ListProductCategoriesWrapperDto { ProductCategories = categories, Pagination = _pagination });
    }

    public async Task<Result<ProductCategoryWrapperDto>> GetByIdAsync(int id)
    {
        var category = await _context.ProductCategories
            .AsNoTracking()
            .Where(u => u.Id == id)
            .ProjectTo<ProductCategoryResponseDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (category == null)
            return Result<ProductCategoryWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        return Result<ProductCategoryWrapperDto>.Success(new ProductCategoryWrapperDto { ProductCategory = category });
    }

    public async Task<Result<ProductCategoryWrapperDto>> CreateAsync(ProductCategoryRequestDto request)
    {
        var category = _mapper.Map<ProductCategory>(request);
        
        _context.ProductCategories.Add(category);
        await _context.SaveChangesAsync();
        
        return await GetByIdAsync(category.Id);
    }

    public async Task<Result<ProductCategoryWrapperDto>> UpdateAsync(int id, ProductCategoryRequestDto request)
    {
        var category = await _context.ProductCategories.FindAsync(id);
        
        if (category == null)
            return Result<ProductCategoryWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        _mapper.Map(request, category);
        _context.ProductCategories.Update(category);
        await _context.SaveChangesAsync();
        
        return await GetByIdAsync(category.Id);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var category = await _context.ProductCategories.FindAsync(id);
        
        if (category == null)
            return Result.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        _context.ProductCategories.Remove(category);
        await _context.SaveChangesAsync();
        
        return Result.Success();
    }
}

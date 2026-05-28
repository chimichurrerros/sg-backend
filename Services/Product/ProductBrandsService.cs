using AutoMapper;
using AutoMapper.QueryableExtensions;
using BackEnd.Infrastructure.Context;
using BackEnd.Constants.Errors;
using BackEnd.Utils;
using BackEnd.DTOs.Responses.ProductBrand;
using Microsoft.EntityFrameworkCore;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Requests.ProductBrand;
using BackEnd.Models;

namespace BackEnd.Services;

public class ProductBrandsService(AppDbContext context, IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ListProductBrandsWrapperDto>> GetAllAsync()
    {
        var brands = await _context.ProductBrands
            .AsNoTracking()
            .ProjectTo<ProductBrandResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Result<ListProductBrandsWrapperDto>.Success(new ListProductBrandsWrapperDto { ProductBrands = brands });
    }

    public async Task<Result<ListProductBrandsWrapperDto>> GetListAsync(ProductBrandQueryDto queryDto)
    {
        var query = _context.ProductBrands.AsNoTracking();
        
        if (queryDto.Id.HasValue)
        {
            query = query.Where(b => b.Id == queryDto.Id.Value);
        }

        if (!string.IsNullOrWhiteSpace(queryDto.Name))
        {
            query = query.Where(b => b.Name.ToLower().Contains(queryDto.Name.ToLower()));
        }

        var totalElements = await query.CountAsync();
        
        var brands = await query
            .OrderBy(v => v.Id)
            .Skip((queryDto.Page - 1) * queryDto.PageSize)
            .Take(queryDto.PageSize)
            .ProjectTo<ProductBrandResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var _pagination = new Pagination(queryDto.Page, queryDto.PageSize, totalElements);
            
        return Result<ListProductBrandsWrapperDto>.Success(new ListProductBrandsWrapperDto { ProductBrands = brands, Pagination = _pagination });
    }

    public async Task<Result<ProductBrandWrapperDto>> GetByIdAsync(int id)
    {
        var brand = await _context.ProductBrands
            .AsNoTracking()
            .Where(u => u.Id == id)
            .ProjectTo<ProductBrandResponseDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (brand == null)
            return Result<ProductBrandWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        return Result<ProductBrandWrapperDto>.Success(new ProductBrandWrapperDto { ProductBrand = brand });
    }

    public async Task<Result<ProductBrandWrapperDto>> CreateAsync(ProductBrandRequestDto request)
    {
        var brand = _mapper.Map<ProductBrand>(request);
        
        _context.ProductBrands.Add(brand);
        await _context.SaveChangesAsync();
        
        return await GetByIdAsync(brand.Id);
    }

    public async Task<Result<ProductBrandWrapperDto>> UpdateAsync(int id, ProductBrandRequestDto request)
    {
        var brand = await _context.ProductBrands.FindAsync(id);
        
        if (brand == null)
            return Result<ProductBrandWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        _mapper.Map(request, brand);
        _context.ProductBrands.Update(brand);
        await _context.SaveChangesAsync();
        
        return await GetByIdAsync(brand.Id);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var brand = await _context.ProductBrands.FindAsync(id);
        
        if (brand == null)
            return Result.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        _context.ProductBrands.Remove(brand);
        await _context.SaveChangesAsync();
        
        return Result.Success();
    }
}

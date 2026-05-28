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
using BackEnd.DTOs.Responses.Supplier;

namespace BackEnd.Services;

public class ProductsService(AppDbContext context, IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ListProductsWrapperDto>> GetAllAsync()
    {
        var products = await _context.Products
            .AsNoTracking()
            .Where(p => p.IsService != true)
            .ProjectTo<ProductResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Result<ListProductsWrapperDto>.Success(new ListProductsWrapperDto { Products = products });
    }

    public async Task<Result<ListProductsWrapperDto>> GetListAsync(ProductQueryDto queryDto)
    {
        var query = _context.Products.AsNoTracking().Where(p => p.IsService != true);

        if (!string.IsNullOrWhiteSpace(queryDto.Name))
        {
            query = query.Where(p => p.Name.ToLower().Contains(queryDto.Name.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(queryDto.BrandName))
        {
            query = query.Where(p => p.ProductBrand != null && p.ProductBrand.Name.ToLower().Contains(queryDto.BrandName.ToLower()));
        }

        if (queryDto.ProductBrandId.HasValue)
        {
            query = query.Where(p => p.ProductBrandId == queryDto.ProductBrandId.Value);
        }

        if (!string.IsNullOrWhiteSpace(queryDto.CategoryName))
        {
            query = query.Where(p => p.ProductCategory != null && p.ProductCategory.Name.ToLower().Contains(queryDto.CategoryName.ToLower()));
        }

        if (queryDto.ProductCategoryId.HasValue)
        {
            query = query.Where(p => p.ProductCategoryId == queryDto.ProductCategoryId.Value);
        }

        if (queryDto.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= queryDto.MinPrice.Value);
        }

        if (queryDto.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= queryDto.MaxPrice.Value);
        }

        if (queryDto.MinQuantity.HasValue)
        {
            query = query.Where(p => p.Stocks.Sum(s => s.Quantity) >= queryDto.MinQuantity.Value);
        }

        if (queryDto.MaxQuantity.HasValue)
        {
            query = query.Where(p => p.Stocks.Sum(s => s.Quantity) <= queryDto.MaxQuantity.Value);
        }

        var totalElements = await query.CountAsync();

        var products = await query
            .OrderBy(v => v.Id)
            .Skip((queryDto.Page - 1) * queryDto.PageSize)
            .Take(queryDto.PageSize)
            .ProjectTo<ProductResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var _pagination = new Pagination(queryDto.Page, queryDto.PageSize, totalElements);

        return Result<ListProductsWrapperDto>.Success(new ListProductsWrapperDto { Products = products, Pagination = _pagination });
    }

    public async Task<Result<ProductWrapperDto>> GetByIdAsync(int id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Where(u => u.Id == id && u.IsService != true)
            .ProjectTo<ProductResponseDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (product == null)
            return Result<ProductWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        return Result<ProductWrapperDto>.Success(new ProductWrapperDto { Product = product });
    }

    public async Task<Result<ProductWrapperDto>> CreateAsync(ProductRequestDto request)
    {
        var product = _mapper.Map<Product>(request);
        product.IsService = false;

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
        product.IsService = false;

        _context.Products.Update(product);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(product.Id);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id || p.IsService != true);

        if (product == null)
            return Result.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<ListSuppliersWrapperDto>> GetAllSuppliers(int idProdut)
    {
        var result = (from product in _context.Products
                      where product.Id == idProdut
                      join sc in _context.SupplierCategories
                        on product.ProductCategoryId equals sc.ProductCategoryId
                      join supplier in _context.Suppliers
                        on sc.SupplierId equals supplier.Id
                      select supplier)
                      .Distinct() // Evita proveedores duplicados si tienen muchos productos
                      .ProjectTo<SupplierResponseDto>(_mapper.ConfigurationProvider)
                      .ToList();

        return Result<ListSuppliersWrapperDto>.Success(new ListSuppliersWrapperDto { Suppliers = result });
    }

    public async Task<Result<ListProductsStockWrapperDto>> GetByBranchIdAsync(int branchId)
{
    var products = await _context.Stocks
        .AsNoTracking()
        .Where(s => s.BranchId == branchId && s.Product.IsService != true)
        .ProjectTo<ProductStockResponseDto>(_mapper.ConfigurationProvider)
        .ToListAsync();
    return Result<ListProductsStockWrapperDto>.Success(
        new ListProductsStockWrapperDto { ProductsStock = products });
}
}
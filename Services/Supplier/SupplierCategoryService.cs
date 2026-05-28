using AutoMapper;
using AutoMapper.QueryableExtensions;
using BackEnd.Infrastructure.Context;
using BackEnd.Constants.Errors;
using BackEnd.Utils;
using BackEnd.Models;
using BackEnd.DTOs.Requests.Supplier;
using BackEnd.DTOs.Responses.Supplier;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services;

public class SupplierCategoryService(AppDbContext context, IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;


    public async Task<Result<SupplierCategoryWrapperDto>> GetBySupplierIdAsync(int supplierId)
    {
        var supplierCategories = await _context.SupplierCategories
            .Where(sc => sc.SupplierId == supplierId)
            .ProjectTo<SupplierCategoryResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Result<SupplierCategoryWrapperDto>.Success(new SupplierCategoryWrapperDto { SupplierCategory = supplierCategories[0] });
    }

    public async Task<Result<SupplierCategoryWrapperDto>> CreateAsync(SupplierCategoryRequestDto request)
    {
        // Validar que el proveedor y la categoría realmente existan en la BD
        var supplierExists = await _context.Suppliers.AnyAsync(s => s.Id == request.SupplierId);
        var categoryExists = await _context.ProductCategories.AnyAsync(c => c.Id == request.ProductCategoryId);

        if (!supplierExists || !categoryExists)
        {
            return Result<SupplierCategoryWrapperDto>.Failure(
                "El Proveedor o la Categoría especificada no existe.",
                ErrorType.Validation
            );
        }

        // Validar que no exista ya esa asociación para no tener duplicados
        var exists = await _context.SupplierCategories
            .AnyAsync(sc => sc.SupplierId == request.SupplierId && sc.ProductCategoryId == request.ProductCategoryId);

        if (exists)
            return Result<SupplierCategoryWrapperDto>.Failure("SupplierCategory.AlreadyExists", ErrorType.Validation);

        var newSupplierCategory = _mapper.Map<SupplierCategory>(request);

        _context.SupplierCategories.Add(newSupplierCategory);
        await _context.SaveChangesAsync();

        // Buscamos el registro recién creado para devolverlo con los datos de la categoría anidados
        var createdDto = await _context.SupplierCategories
            .Where(sc => sc.Id == newSupplierCategory.Id)
            .ProjectTo<SupplierCategoryResponseDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        return Result<SupplierCategoryWrapperDto>.Success(new SupplierCategoryWrapperDto { SupplierCategory = createdDto! });
    }

    public async Task<Result<bool>> DeleteAsync(int id)
    {
        var supplierCategory = await _context.SupplierCategories.FindAsync(id);

        if (supplierCategory == null)
            return Result<bool>.Failure("SupplierCategory.NotFound", ErrorType.NotFound);

        _context.SupplierCategories.Remove(supplierCategory);
        await _context.SaveChangesAsync();

        return Result<bool>.Success(true);
    }
}
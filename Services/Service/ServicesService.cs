using AutoMapper;
using AutoMapper.QueryableExtensions;
using BackEnd.Infrastructure.Context;
using BackEnd.Constants.Errors;
using BackEnd.Utils;
using BackEnd.DTOs.Responses.Service;
using Microsoft.EntityFrameworkCore;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Requests.Service;
using BackEnd.Models;

namespace BackEnd.Services;

public class ServicesService(AppDbContext context, IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ListServiceWrapperDto>> GetAllAsync()
    {
        var products = await _context.Products
            .AsNoTracking()
            .Where(p => p.IsService == true)
            .ProjectTo<ServiceResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Result<ListServiceWrapperDto>.Success(new ListServiceWrapperDto { Services = products });
    }

    public async Task<Result<ListServiceWrapperDto>> GetListAsync(ServiceQueryDto queryDto)
    {
        var query = _context.Products.AsNoTracking().Where(p => p.IsService == true);

        if (queryDto.Id.HasValue)
        {
            query = query.Where(p => p.Id == queryDto.Id.Value);
        }

        if (!string.IsNullOrWhiteSpace(queryDto.Name))
        {
            query = query.Where(p => p.Name.ToLower().Contains(queryDto.Name.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(queryDto.Description))
        {
            query = query.Where(p => p.Description.ToLower().Contains(queryDto.Description.ToLower()));
        }

        if (queryDto.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= queryDto.MinPrice.Value);
        }

        if (queryDto.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= queryDto.MaxPrice.Value);
        }

        if (queryDto.MinCost.HasValue)
        {
            query = query.Where(p => p.Cost >= queryDto.MinCost.Value);
        }

        if (queryDto.MaxCost.HasValue)
        {
            query = query.Where(p => p.Cost <= queryDto.MaxCost.Value);
        }

        var totalElements = await query.CountAsync();

        var products = await query
            .OrderBy(v => v.Id)
            .Skip((queryDto.Page - 1) * queryDto.PageSize)
            .Take(queryDto.PageSize)
            .ProjectTo<ServiceResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var _pagination = new Pagination(queryDto.Page, queryDto.PageSize, totalElements);

        return Result<ListServiceWrapperDto>.Success(new ListServiceWrapperDto { Services = products, Pagination = _pagination });
    }

    public async Task<Result<ServiceWrapperDto>> GetByIdAsync(int id)
    {
        var product = await _context.Products
            .AsNoTracking()
            .Where(u => u.Id == id && u.IsService == true)
            .ProjectTo<ServiceResponseDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (product == null)
            return Result<ServiceWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        return Result<ServiceWrapperDto>.Success(new ServiceWrapperDto { Service = product });
    }

    public async Task<Result<ServiceWrapperDto>> CreateAsync(ServiceRequestDto request)
    {
        var product = _mapper.Map<Product>(request);
        product.IsService = true;

        _context.Products.Add(product);
        await _context.SaveChangesAsync();

        // Reload to get names for the DTO
        return await GetByIdAsync(product.Id);
    }

    public async Task<Result<ServiceWrapperDto>> UpdateAsync(int id, ServiceRequestDto request)
    {
        var product = await _context.Products.FindAsync(id);

        if (product == null)
            return Result<ServiceWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        _mapper.Map(request, product);
        product.IsService = true;

        _context.Products.Update(product);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(product.Id);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id || p.IsService == true);

        if (product == null)
            return Result.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();

        return Result.Success();
    }
}
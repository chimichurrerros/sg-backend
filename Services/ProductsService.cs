using AutoMapper;
using BackEnd.DTOs.Requests.Product;
using BackEnd.Infrastructure.Context;
using BackEnd.Models;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services;

public class ProductsService(AppDbContext context, IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<List<Product>> GetListAsync()
    {
        return await _context.Products.ToListAsync();
    }

    public async Task<Product?> GetByIdAsync(int id)
    {
        return await _context.Products.FindAsync(id);
    }

    public async Task<Product> CreateAsync(ProductRequestDto product)
    {
        _context.Products.Add(_mapper.Map<Product>(product));
        await _context.SaveChangesAsync();
        return _mapper.Map<Product>(product);
    }

    public async Task<Product> UpdateAsync(ProductRequestDto product)
    {
        _context.Products.Update(_mapper.Map<Product>(product));
        await _context.SaveChangesAsync();
        return _mapper.Map<Product>(product);
    }

    public async Task<Product> DeleteAsync(ProductRequestDto product)
    {
        _context.Products.Remove(_mapper.Map<Product>(product));
        await _context.SaveChangesAsync();
        return _mapper.Map<Product>(product);
    }
}
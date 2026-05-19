using AutoMapper;
using AutoMapper.QueryableExtensions;
using BackEnd.Infrastructure.Context;
using BackEnd.Constants.Errors;
using BackEnd.Utils;
using BackEnd.DTOs.Responses.Bank;
using Microsoft.EntityFrameworkCore;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Requests.Bank;
using BackEnd.Models;

namespace BackEnd.Services;

public class BankService(AppDbContext context, IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ListBanksWrapperDto>> GetAllAsync()
    {
        var Banks = await _context.Banks
            .AsNoTracking()
            .ProjectTo<BankResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Result<ListBanksWrapperDto>.Success(new ListBanksWrapperDto { Banks = Banks });
    }

    public async Task<Result<ListBanksWrapperDto>> GetListAsync(PaginationRequestDto pagination)
    {
        var query = _context.Banks.AsNoTracking();

        var totalElements = await query.CountAsync();

        var Banks = await query
            .OrderBy(v => v.Id)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectTo<BankResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var _pagination = new Pagination(pagination.Page, pagination.PageSize, totalElements);

        return Result<ListBanksWrapperDto>.Success(new ListBanksWrapperDto { Banks = Banks, Pagination = _pagination });
    }

    public async Task<Result<BankWrapperDto>> GetByIdAsync(int id)
    {
        var bank = await _context.Banks
            .AsNoTracking()
            .Where(u => u.Id == id)
            .ProjectTo<BankResponseDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (bank == null)
            return Result<BankWrapperDto>.Failure(BankError.BankNotFound, ErrorType.NotFound);

        return Result<BankWrapperDto>.Success(new BankWrapperDto { Bank = bank });
    }

    public async Task<Result<BankWrapperDto>> CreateAsync(BankRequestDto request)
    {
        var bank = _mapper.Map<Bank>(request);

        _context.Banks.Add(bank);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(bank.Id);
    }

    public async Task<Result<BankWrapperDto>> UpdateAsync(int id, UpdateBankRequestDto request)
    {
        var bank = await _context.Banks.FindAsync(id);

        if (bank == null)
            return Result<BankWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        _mapper.Map(request, bank);
        _context.Banks.Update(bank);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(bank.Id);
    }

    	public async Task<Result> ToggleStatusAsync(int id)
	{
		var bank = await _context.Banks.FirstOrDefaultAsync(u => u.Id == id);

		if (bank == null)
			return Result.Failure(BankError.BankNotFound, ErrorType.NotFound);

		bank.IsActive = !bank.IsActive;

		_context.Banks.Update(bank);
		await _context.SaveChangesAsync();

		return Result.Success();
	}
}

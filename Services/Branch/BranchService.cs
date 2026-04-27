using AutoMapper;
using AutoMapper.QueryableExtensions;
using BackEnd.Infrastructure.Context;
using BackEnd.Constants.Errors;
using BackEnd.Utils;
using BackEnd.DTOs.Responses.Branch;
using Microsoft.EntityFrameworkCore;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Requests.Branch;
using BackEnd.Models;

namespace BackEnd.Services;

public class BranchService(AppDbContext context, IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ListBranchesWrapperDto>> GetAllAsync()
    {
        var branches = await _context.Branches
            .AsNoTracking()
            .ProjectTo<BranchResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Result<ListBranchesWrapperDto>.Success(new ListBranchesWrapperDto { Branches = branches });
    }

    public async Task<Result<ListBranchesWrapperDto>> GetListAsync(PaginationRequestDto pagination)
    {
        var query = _context.Branches.AsNoTracking();

        var totalElements = await query.CountAsync();

        var branches = await query
            .OrderBy(v => v.Id)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectTo<BranchResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var _pagination = new Pagination(pagination.Page, pagination.PageSize, totalElements);

        return Result<ListBranchesWrapperDto>.Success(new ListBranchesWrapperDto { Branches = branches, Pagination = _pagination });
    }

    public async Task<Result<BranchWrapperDto>> GetByIdAsync(int id)
    {
        var branch = await _context.Branches
            .AsNoTracking()
            .Where(u => u.Id == id)
            .ProjectTo<BranchResponseDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (branch == null)
            return Result<BranchWrapperDto>.Failure(BranchError.BranchNotFound, ErrorType.NotFound);

        return Result<BranchWrapperDto>.Success(new BranchWrapperDto { Branch = branch });
    }

    public async Task<Result<BranchWrapperDto>> CreateAsync(BranchRequestDto request)
    {
        var branch = _mapper.Map<Branch>(request);

        _context.Branches.Add(branch);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(branch.Id);
    }

    public async Task<Result<BranchWrapperDto>> UpdateAsync(int id, BranchRequestDto request)
    {
        var branch = await _context.Branches.FindAsync(id);

        if (branch == null)
            return Result<BranchWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        _mapper.Map(request, branch);
        _context.Branches.Update(branch);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(branch.Id);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var branch = await _context.Branches.FindAsync(id);

        if (branch == null)
            return Result.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        _context.Branches.Remove(branch);
        await _context.SaveChangesAsync();

        return Result.Success();
    }
}

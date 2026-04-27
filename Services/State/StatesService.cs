using AutoMapper;
using AutoMapper.QueryableExtensions;
using BackEnd.Infrastructure.Context;
using BackEnd.Constants.Errors;
using BackEnd.Utils;
using BackEnd.DTOs.Responses.State;
using Microsoft.EntityFrameworkCore;
using BackEnd.DTOs.Requests.Pagination;
using BackEnd.DTOs.Requests.State;
using BackEnd.Models;

namespace BackEnd.Services;

public class StatesService(AppDbContext context, IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ListStatesWrapperDto>> GetAllAsync()
    {
        var states = await _context.States
            .AsNoTracking()
            .ProjectTo<StateResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        return Result<ListStatesWrapperDto>.Success(new ListStatesWrapperDto { States = states });
    }

    public async Task<Result<ListStatesWrapperDto>> GetListAsync(PaginationRequestDto pagination)
    {
        var query = _context.States.AsNoTracking();
        
        var totalElements = await query.CountAsync();
        
        var states = await query
            .OrderBy(v => v.Id)
            .Skip((pagination.Page - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .ProjectTo<StateResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var _pagination = new Pagination(pagination.Page, pagination.PageSize, totalElements);
            
        return Result<ListStatesWrapperDto>.Success(new ListStatesWrapperDto { States = states, Pagination = _pagination });
    }

    public async Task<Result<StateWrapperDto>> GetByIdAsync(int id)
    {
        var state = await _context.States
            .AsNoTracking()
            .Where(u => u.Id == id)
            .ProjectTo<StateResponseDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (state == null)
            return Result<StateWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        return Result<StateWrapperDto>.Success(new StateWrapperDto { State = state });
    }

    public async Task<Result<StateWrapperDto>> CreateAsync(StateRequestDto request)
    {
        var state = _mapper.Map<State>(request);
        
        _context.States.Add(state);
        await _context.SaveChangesAsync();
        
        return await GetByIdAsync(state.Id);
    }

    public async Task<Result<StateWrapperDto>> UpdateAsync(int id, StateRequestDto request)
    {
        var state = await _context.States.FindAsync(id);
        
        if (state == null)
            return Result<StateWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        _mapper.Map(request, state);
        _context.States.Update(state);
        await _context.SaveChangesAsync();
        
        return await GetByIdAsync(state.Id);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var state = await _context.States.FindAsync(id);
        
        if (state == null)
            return Result.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        _context.States.Remove(state);
        await _context.SaveChangesAsync();
        
        return Result.Success();
    }
}

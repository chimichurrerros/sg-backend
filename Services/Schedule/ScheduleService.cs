using AutoMapper;
using AutoMapper.QueryableExtensions;
using BackEnd.Constants.Errors;
using BackEnd.DTOs.Requests.Schedule;
using BackEnd.DTOs.Requests.Organization;
using BackEnd.DTOs.Responses.Schedule;
using BackEnd.Infrastructure.Context;
using BackEnd.Models;
using BackEnd.Utils;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services;

public class ScheduleService(AppDbContext context, IMapper mapper)
{
    private readonly AppDbContext _context = context;
    private readonly IMapper _mapper = mapper;

    public async Task<Result<ListSchedulesWrapperDto>> GetAllAsync(OrganizationQueryDto query)
    {
        var normalizedSearch = query.Search?.Trim().ToLowerInvariant();

        var schedulesQuery = _context.Schedules
            .AsNoTracking()
            .Where(s => string.IsNullOrWhiteSpace(query.Search) ||
                        s.ArrivalTime.ToString().Contains(query.Search) ||
                        s.DepartureTime.ToString().Contains(query.Search) ||
                        (normalizedSearch == "morning" && s.ScheduleType == ScheduleTypeEnum.Morning) ||
                        (normalizedSearch == "afternoon" && s.ScheduleType == ScheduleTypeEnum.Afternoon) ||
                        (normalizedSearch == "night" && s.ScheduleType == ScheduleTypeEnum.Night) ||
                        (normalizedSearch == "fulltime" && s.ScheduleType == ScheduleTypeEnum.FullTime) ||
                        (normalizedSearch == "parttime" && s.ScheduleType == ScheduleTypeEnum.PartTime) ||
                        (normalizedSearch == "turno mañana" && s.ScheduleType == ScheduleTypeEnum.Morning) ||
                        (normalizedSearch == "turno tarde" && s.ScheduleType == ScheduleTypeEnum.Afternoon) ||
                        (normalizedSearch == "turno noche" && s.ScheduleType == ScheduleTypeEnum.Night) ||
                        (normalizedSearch == "jornada completa" && s.ScheduleType == ScheduleTypeEnum.FullTime) ||
                        (normalizedSearch == "medio tiempo" && s.ScheduleType == ScheduleTypeEnum.PartTime));

        schedulesQuery = ApplySort(schedulesQuery, query.SortBy, query.SortOrder);

        var totalElements = await schedulesQuery.CountAsync();

        var schedules = await schedulesQuery
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .ProjectTo<ScheduleResponseDto>(_mapper.ConfigurationProvider)
            .ToListAsync();

        var pagination = new Pagination(query.Page, query.PageSize, totalElements);

        return Result<ListSchedulesWrapperDto>.Success(new ListSchedulesWrapperDto { Schedules = schedules, Pagination = pagination });
    }

    public async Task<Result<ScheduleWrapperDto>> GetByIdAsync(int id)
    {
        var schedule = await _context.Schedules
            .AsNoTracking()
            .Where(s => s.Id == id)
            .ProjectTo<ScheduleResponseDto>(_mapper.ConfigurationProvider)
            .FirstOrDefaultAsync();

        if (schedule == null)
            return Result<ScheduleWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        return Result<ScheduleWrapperDto>.Success(new ScheduleWrapperDto { Schedule = schedule });
    }

    public async Task<Result<ScheduleWrapperDto>> CreateAsync(ScheduleRequestDto request)
    {
        var schedule = _mapper.Map<Schedule>(request);

        _context.Schedules.Add(schedule);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(schedule.Id);
    }

    public async Task<Result<ScheduleWrapperDto>> UpdateAsync(int id, ScheduleRequestDto request)
    {
        var schedule = await _context.Schedules.FindAsync(id);

        if (schedule == null)
            return Result<ScheduleWrapperDto>.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        _mapper.Map(request, schedule);
        _context.Schedules.Update(schedule);
        await _context.SaveChangesAsync();

        return await GetByIdAsync(schedule.Id);
    }

    public async Task<Result> DeleteAsync(int id)
    {
        var schedule = await _context.Schedules.FindAsync(id);

        if (schedule == null)
            return Result.Failure(ApplicationError.NotFound, ErrorType.NotFound);

        _context.Schedules.Remove(schedule);
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    private static IQueryable<Schedule> ApplySort(IQueryable<Schedule> query, string? sortBy, string? sortOrder)
    {
        var desc = string.Equals(sortOrder, "desc", StringComparison.OrdinalIgnoreCase);
        return (sortBy ?? "id").ToLowerInvariant() switch
        {
            "arrivaltime" => desc ? query.OrderByDescending(s => s.ArrivalTime) : query.OrderBy(s => s.ArrivalTime),
            "departuretime" => desc ? query.OrderByDescending(s => s.DepartureTime) : query.OrderBy(s => s.DepartureTime),
            "numberofhours" => desc ? query.OrderByDescending(s => s.NumberOfHours) : query.OrderBy(s => s.NumberOfHours),
            "scheduletype" => desc ? query.OrderByDescending(s => s.ScheduleType) : query.OrderBy(s => s.ScheduleType),
            _ => desc ? query.OrderByDescending(s => s.Id) : query.OrderBy(s => s.Id)
        };
    }
}
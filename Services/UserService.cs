using AutoMapper;
using AutoMapper.QueryableExtensions;
using BackEnd.Infrastructure.Context;
using BackEnd.Constants.Errors;
using BackEnd.Utils;
using BackEnd.DTOs.Responses.User;
using Microsoft.EntityFrameworkCore;
using BackEnd.DTOs.Requests.Pagination;

namespace BackEnd.Services;

public class UserService(AppDbContext context, IMapper mapper)
{
	private readonly AppDbContext _context = context;
	private readonly IMapper _mapper = mapper;

	public async Task<Result<ListUsersWrapperDto>> GetListAsync(PaginationRequestDto pagination)
	{
		// !NOTE: Projecto dont work with nested 
		// !collections, so we need to load the users and then map them to the DTOs

		// var users = await _context.Users
		// 	.ProjectTo<ListUsersResponseDto>(_mapper.ConfigurationProvider)
		// 	.ToListAsync();

		var _users = await _context.Users
			.OrderBy(v => v.Id)
			.Skip((pagination.Page - 1) * pagination.PageSize)
			.Take(pagination.PageSize)
			.ProjectTo<UserResponseDto>(_mapper.ConfigurationProvider)
			.ToListAsync();

		var _pagination = new Pagination(pagination.Page, pagination.PageSize, _users.Count);
			
		return Result<ListUsersWrapperDto>.Success(new ListUsersWrapperDto { Users = _users, Pagination = _pagination });
	}

	public async Task<Result<UserWrapperDto>> GetByIdAsync(string id)
	{
		var user = await _context.Users
			.Where(u => u.Id.ToString() == id)
			.ProjectTo<UserWrapperDto>(_mapper.ConfigurationProvider)
			.FirstOrDefaultAsync();

		if (user == null)
			return Result<UserWrapperDto>.Failure(AuthError.UserNotFound, ErrorType.NotFound);

		return Result<UserWrapperDto>.Success(user);
	}
}
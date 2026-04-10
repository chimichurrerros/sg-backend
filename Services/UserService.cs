using AutoMapper;
using AutoMapper.QueryableExtensions;
using BackEnd.Infrastructure.Context;
using BackEnd.Constants.Errors;
using BackEnd.Utils;
using BackEnd.DTOs.Responses.User;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services;

public class UserService(AppDbContext context, IMapper mapper)
{
	private readonly AppDbContext _context = context;
	private readonly IMapper _mapper = mapper;

	public async Task<Result<UserWrapperDto>> GetProfileAsync(string? userId)
	{
		var profile = await _context.Users
			.Where(u => u.Id.ToString() == userId)
			.ProjectTo<UserWrapperDto>(_mapper.ConfigurationProvider)
			.FirstOrDefaultAsync();

		if (profile == null)
			return Result<UserWrapperDto>.Failure(AuthError.UserNotFound, ErrorType.NotFound);

		return Result<UserWrapperDto>.Success(profile);
	}

	public async Task<Result<ListUsersWrapperDto>> GetListAsync()
	{
		// !NOTE: Projecto dont work with nested 
		// !collections, so we need to load the users and then map them to the DTOs

		// var users = await _context.Users
		// 	.ProjectTo<ListUsersResponseDto>(_mapper.ConfigurationProvider)
		// 	.ToListAsync();

		var _users = await _context.Users
				.ProjectTo<UserResponseDto>(_mapper.ConfigurationProvider)
				.ToListAsync();

		var result = new ListUsersWrapperDto { Users = _users };

		return Result<ListUsersWrapperDto>.Success(result);
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

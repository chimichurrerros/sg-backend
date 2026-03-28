using AutoMapper;
using AutoMapper.QueryableExtensions;
using BackEnd.Infrastructure.Context;
using BackEnd.Constants.Errors;
using BackEnd.Constants.Messages;
using BackEnd.Models;
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
			.ProjectTo<UserResponseDto>(_mapper.ConfigurationProvider)
			.FirstOrDefaultAsync();

		if (profile == null)
            return Result<UserWrapperDto>.Failure(AuthError.UserNotFound);

        return Result<UserWrapperDto>.Success(new UserWrapperDto { User = profile }	);
	}
}

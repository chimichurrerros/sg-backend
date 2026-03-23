using AutoMapper;
using AutoMapper.QueryableExtensions;
using BackEnd.Context;
using BackEnd.Constants.Errors;
using BackEnd.Constants.Messages;
using BackEnd.Models.Responses.Application;
using BackEnd.Models.Responses.User;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services;

public class UsuarioService(AppDbContext context, IMapper mapper)
{
	private readonly AppDbContext _context = context;
	private readonly IMapper _mapper = mapper;

	public async Task<ApiResponseDto<UserResponseDto?>> GetProfileAsync(string? userId)
	{
		var profile = await _context.Users
			.Where(u => u.Id.ToString() == userId)
			.ProjectTo<UserResponseDto>(_mapper.ConfigurationProvider)
			.FirstOrDefaultAsync();

		return new ApiResponseDto<UserResponseDto?>
		{
			Success = profile != null,
			Message = profile != null
				? UserMessage.UserProfileRetrieved
				: AuthError.UserNotFound,
			Data = profile
		};
	}
}

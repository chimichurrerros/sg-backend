using AutoMapper;
using AutoMapper.QueryableExtensions;
using BackEnd.Context;
using BackEnd.Models.Constants;
using BackEnd.Models.Responses;
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
				? ApplicationMessages.Success.UserProfileRetrieved
				: ApplicationError.NotFoundError.UserNotFound,
			Data = profile
		};
	}
}

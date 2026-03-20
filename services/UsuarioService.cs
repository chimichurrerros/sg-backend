using AutoMapper;
using BackEnd.Context;
using BackEnd.Models.Responses.User;
using Microsoft.EntityFrameworkCore;

namespace BackEnd.Services;

public interface IUsuarioService
{
	Task<UserResponseDto?> GetProfileAsync(string? userId);
}

public class UsuarioService(AppDbContext context, IMapper mapper) : IUsuarioService
{
	private readonly AppDbContext _context = context;
	private readonly IMapper _mapper = mapper;

	public async Task<UserResponseDto?> GetProfileAsync(string? userId)
	{
		if (string.IsNullOrWhiteSpace(userId))
		{
			return null;
		}

		var profile = await _context.Users
			.Where(u => u.Id.ToString() == userId)
			.Include(u => u.Role)
			.Include(u => u.PhoneNumbers)
			.FirstOrDefaultAsync();

		return profile is null ? null : _mapper.Map<UserResponseDto>(profile);
	}
}

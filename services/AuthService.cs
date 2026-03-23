using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using BackEnd.Context;
using BackEnd.Entities;
using BackEnd.Constants.Errors;
using BackEnd.Constants.Messages;
using BackEnd.Models.Requests.Auth;
using BackEnd.Models.Responses;
using BackEnd.Models.Responses.User;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

namespace BackEnd.Services;

public class AuthService(AppDbContext context, IConfiguration config, IMapper mapper, IHttpContextAccessor httpContextAccessor)
{
    private readonly AppDbContext _context = context;
    private readonly IConfiguration _config = config;
    private readonly IMapper _mapper = mapper;
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;
    private const int _tokenExpirationDays = 30;

    public async Task<ApiResponseDto<object>> RegisterAsync(RegisterRequestDto request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return new ApiResponseDto<object>
            {
                Success = false,
                Message = AuthError.InvalidCredentials,
                Errors = new { Email = new[] { EmailError.EmailAlreadyExists } }
            };
        }

        var user = new User
        {
            Name = request.Name,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return new ApiResponseDto<object>
        {
            Success = true,
            Message = AuthMessage.UserRegistered
        };
    }

    public async Task<ApiResponseDto<object>> LoginAsync(LoginRequestDto request)
    {
        var user = await _context.Users
            .Include(u => u.PhoneNumbers)
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        // User not found or password does not match
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return new ApiResponseDto<object>
            {
                Success = false,
                Message = AuthError.InvalidCredentials,
                Errors = new { Authentication = new[] { AuthError.InvalidCredentials } }
            };
        }

        var token = CreateToken(user);
        SetAuthCookie(token);

        return new ApiResponseDto<object>
        {
            Success = true,
            Message = AuthMessage.LoginSuccessful,
            Data = _mapper.Map<UserResponseDto>(user)
        };
    }

    public async Task<ApiResponseDto<object>> Logout()
    {
        DeleteAuthCookie();
        return new ApiResponseDto<object>
        {
            Success = true,
            Message = AuthMessage.UserLoggedOut
        };
    }

    // ╭────────────────────────────────────────────────────────╮
    // │                      AUX FUNTIONS                      │
    // ╰────────────────────────────────────────────────────────╯

    private string CreateToken(User user)
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Surname, user.LastName),
            new(ClaimTypes.Role, user.Role!.Name)
        };

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config.GetSection("Jwt:Key").Value!));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(_tokenExpirationDays),
            SigningCredentials = creds
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    private void SetAuthCookie(string token)
    {
        var options = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.None,
            Expires = DateTime.Now.AddDays(_tokenExpirationDays)
        };

        _httpContextAccessor
        .HttpContext?
        .Response.Cookies.Append("current_user", token, options);
    }

    private void DeleteAuthCookie()
    {
        _httpContextAccessor
        .HttpContext?
        .Response.Cookies.Delete("current_user");
    }
}
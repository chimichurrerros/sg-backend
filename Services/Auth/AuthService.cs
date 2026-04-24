using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using AutoMapper;
using BackEnd.Infrastructure.Context;
using BackEnd.Constants.Errors;
using BackEnd.DTOs.Requests.Auth;
using BackEnd.Models;
using BackEnd.Utils;
using BackEnd.DTOs.Responses.User;
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

    public async Task<Result> RegisterAsync(RegisterRequestDto request)
    {
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return Result.Failure(AuthError.InvalidCredentials, new Dictionary<string, string[]> {
                {"Email", new[] {EmailError.EmailAlreadyExists}}
            }, ErrorType.Validation);
        }

        var defaultRole = 1;

        var user = new User
        {
            Name = request.Name,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            RoleId = defaultRole
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return Result.Success();
    }

    public async Task<Result<UserWrapperDto>> LoginAsync(LoginRequestDto request)
    {
        var user = await _context.Users
            .AsNoTracking()
            .Include(u => u.Role)
                .ThenInclude(r => r!.Permissions)
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        // User not found or password does not match
        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return Result<UserWrapperDto>.Failure(AuthError.InvalidCredentials, new Dictionary<string, string[]> {
                { "Authentication", new[] { AuthError.InvalidCredentials } }
            }, ErrorType.Validation);
        }

        var token = CreateToken(user);
        SetAuthCookie(token);

        return Result<UserWrapperDto>.Success(_mapper.Map<UserWrapperDto>(user));
    }

    public async Task<Result> Logout()
    {
        DeleteAuthCookie();
        return Result.Success();
    }

    // ╭────────────────────────────────────────────────────────╮
    // │                      AUX FUNTIONS                      │
    // ╰────────────────────────────────────────────────────────╯

    /**
     * Create a JWT token for the given user
     * @param user The user to create the token for
     * @return The JWT token
     */
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

        if (user.Role!.Permissions is { } permissions) // I need this to suppress a warning
        {
            foreach (var permission in permissions)
            {
                claims.Add(new Claim("Permission", permission.Name));
            }
        }

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
using System.ComponentModel.DataAnnotations;
using BackEnd.Constants.Errors;

namespace BackEnd.DTOs.Requests.Auth;

public class LoginRequestDto
{
    [Required(ErrorMessage = EmailError.EmailRequired)]
    [EmailAddress(ErrorMessage = EmailError.InvalidEmail)]
    public string Email { get; set; } = null!;
    [Required(ErrorMessage = UserError.PasswordRequired)]
    public string Password { get; set; } = null!;
}
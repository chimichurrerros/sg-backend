using System.ComponentModel.DataAnnotations;
using BackEnd.Constants.Errors;

namespace BackEnd.DTOs.Requests.Auth;

public class RegisterRequestDto
{
    [Required(ErrorMessage = UserError.NameRequired)]
    public string Name { get; set; } = null!;
    [Required(ErrorMessage = UserError.LastNameRequired)]
    public string LastName { get; set; } = null!;
    [Required(ErrorMessage = EmailError.EmailRequired)]
    [EmailAddress(ErrorMessage = EmailError.InvalidEmail)]
    public string Email { get; set; } = null!;
    [Required(ErrorMessage = UserError.PasswordRequired)]
    [MinLength(8, ErrorMessage = UserError.PasswordLength)]
    public string Password { get; set; } = null!;
}
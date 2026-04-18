using System.ComponentModel.DataAnnotations;
using BackEnd.Constants.Errors;

namespace BackEnd.DTOs.Requests.User;

public class UpdateUserRequestDto
{
    public string? Name { get; set; }

    public string? LastName { get; set; }

    [EmailAddress(ErrorMessage = EmailError.InvalidEmail)]
    public string? Email { get; set; }

    public int? RoleId { get; set; }
    public bool? IsActive { get; set; }
}

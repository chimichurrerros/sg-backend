using System.ComponentModel.DataAnnotations;
using BackEnd.Constants.Errors;

namespace BackEnd.DTOs.Requests.Customer;

public class CreateCustomerRequestDto
{
    [Required(ErrorMessage = CustomerError.DocumentNumberRequired)]
    public string DocumentNumber { get; set; } = null!;

    [Required(ErrorMessage = CustomerError.BusinessNameRequired)]
    public string BusinessName { get; set; } = null!;

    public string? FantasyName { get; set; }

    [EmailAddress(ErrorMessage = EmailError.InvalidEmail)]
    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public bool IsActive { get; set; } = true;

    public decimal CreditLimit { get; set; }
}

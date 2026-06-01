using System.ComponentModel.DataAnnotations;
using BackEnd.Constants.Errors;

namespace BackEnd.DTOs.Requests.Customer;

public class CreateCustomerRequestDto
{
    [Required(ErrorMessage = CustomerError.NameRequired)]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = CustomerError.RucRequired)]
    public string Ruc { get; set; } = null!;

    public DateOnly? BirthDate { get; set; }

    public string? Email { get; set; }
}

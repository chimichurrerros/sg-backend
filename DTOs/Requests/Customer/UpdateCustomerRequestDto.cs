namespace BackEnd.DTOs.Requests.Customer;

public class UpdateCustomerRequestDto
{
    public string? Name { get; set; }
    public string? Ruc { get; set; }
    public DateOnly? BirthDate { get; set; }
    public string? Email { get; set; }
}

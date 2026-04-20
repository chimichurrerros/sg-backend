using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.Customer;

public class CustomerResponseDto
{
    public int Id { get; set; }
    public int EntityId { get; set; }
    public decimal CreditLimit { get; set; }
    public virtual object? Entity { get; set; }
}

public class CustomerWrapperDto
{
    public CustomerResponseDto Customer { get; set; } = null!;
}

public class ListCustomersWrapperDto
{
    public List<CustomerResponseDto> Customers { get; set; } = [];
    public Pagination Pagination { get; set; } = null!;
}

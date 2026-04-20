using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.Customer;

public class CustomerResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Ruc { get; set; } = null!;
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

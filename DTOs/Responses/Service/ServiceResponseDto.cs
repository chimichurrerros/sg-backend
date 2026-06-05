using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.Service;


public class ServiceResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string Description { get; set; } = null!;
    public string Barcode { get; set; } = null!;
    public decimal Price { get; set; }
    public decimal Cost { get; set; }
}

public class ServiceWrapperDto
{
    public ServiceResponseDto Service { get; set; } = null!;
}

// Crear todo los ListWrapper con paginacion
public class ListServiceWrapperDto
{
    public List<ServiceResponseDto> Services { get; set; } = [];
    public Pagination Pagination { get; set; } = null!;
}

namespace BackEnd.DTOs.Requests.Service;

public partial class ServiceRequestDto
{

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Barcode { get; set; } = null!;

    public decimal Price { get; set; }

    public decimal Cost { get; set; }

}

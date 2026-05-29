using System.ComponentModel.DataAnnotations;

namespace BackEnd.DTOs.Requests.Product;

public partial class ProductRequestDto
{
    public int ProductCategoryId { get; set; }

    public int ProductBrandId { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string Barcode { get; set; } = null!;

    public decimal Price { get; set; }
    public bool? IsDeleted { get; set; }

    public decimal Cost { get; set; }

    public decimal MinimumStock { get; set; }

    [Range(typeof(decimal), "0", "100")]
    public decimal TaxRate { get; set; } = 10m;
}

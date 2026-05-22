using System.ComponentModel.DataAnnotations;

namespace BackEnd.DTOs.Requests.SupplierQuote;

public class SupplierQuoteDetailRequestDto
{
    [Required]
    public int ProductId { get; set; }

    [Range(typeof(decimal), "0.00", "79228162514264337593543950335")]
    public decimal QuantityAvailable { get; set; }

    [Range(typeof(decimal), "0.00", "79228162514264337593543950335")]
    public decimal Price { get; set; }
}

public class CreateSupplierQuoteRequestDto
{
    [Required]
    public int SupplierId { get; set; }

    [Required]
    public int PurchaseRequestId { get; set; }

    [MinLength(1)]
    public List<SupplierQuoteDetailRequestDto> Details { get; set; } = [];
}

public class UpdateSupplierQuoteRequestDto
{
    public int? SupplierId { get; set; }

    public int? PurchaseRequestId { get; set; }

    public List<SupplierQuoteDetailRequestDto>? Details { get; set; }

    public int? State { get; set; }
}

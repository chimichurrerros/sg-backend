using System.ComponentModel.DataAnnotations;

namespace BackEnd.DTOs.Requests.PurchaseOrder;

public class PurchaseOrderDetailRequestDto
{
    [Required]
    public int ProductId { get; set; }

    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal QuantityOrdered { get; set; }

    public int? SupplierQuoteDetailId { get; set; }
}

public class CreatePurchaseOrderRequestDto
{
    [Required]
    public int PurchaseRequestId { get; set; }

    [MinLength(1)]
    public List<PurchaseOrderDetailRequestDto> Details { get; set; } = [];
}

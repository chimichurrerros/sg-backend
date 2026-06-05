using BackEnd.DTOs.Responses.PurchaseReceipt;
using BackEnd.Models;

namespace BackEnd.DTOs.Mappings;

public static class PurchaseReceiptMapper
{
    public static PurchaseReceiptDetailResponseDto MapDetail(PurchaseReceiptDetail detail)
    {
        return new PurchaseReceiptDetailResponseDto
        {
            Id = detail.Id,
            ProductId = detail.ProductId,
            ProductName = detail.Product?.Name ?? string.Empty,
            Quantity = detail.Quantity,
            Price = detail.Price,
            TaxRate = detail.TaxRate,
            LineTotal = detail.Quantity * detail.Price
        };
    }

    public static PurchaseReceiptResponseDto MapReceipt(PurchaseReceipt receipt)
    {
        return new PurchaseReceiptResponseDto
        {
            Id = receipt.Id,
            PurchaseOrderForSupplierId = receipt.PurchaseOrderForSupplierId,
            BillId = receipt.BillId,
            BranchId = receipt.BranchId,
            BranchName = receipt.Branch?.Name ?? string.Empty,
            SupplierId = receipt.SupplierId,
            SupplierName = receipt.Supplier?.BusinessName ?? string.Empty,
            Number = receipt.Number,
            Stamp = receipt.Stamp,
            Date = receipt.Date,
            Observation = receipt.Observation,
            Total = receipt.Total,
            TaxTotal = receipt.TaxTotal,
            Details = receipt.PurchaseReceiptDetails.Select(MapDetail).ToList()
        };
    }
}

using System;
using System.Collections.Generic;

namespace BackEnd.DTOs.Requests.PurchaseReceipt;

public class CreatePurchaseReceiptDto
{
    public int PurchaseOrderForSupplierId { get; set; }
    public string BillNumber { get; set; } = null!;
    public string Stamp { get; set; } = null!;
    public DateOnly Date { get; set; }
    public int SupplierId { get; set; }
    public int BranchId { get; set; } // Establecimiento / Depósito
    public string? Observation { get; set; }

    public List<CreatePurchaseReceiptDetailDto> Details { get; set; } = new();
}

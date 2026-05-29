using System;
using System.Collections.Generic;

namespace BackEnd.DTOs.Requests.PurchaseReturn;

public class CreatePurchaseReturnDto
{
    public int PurchaseOrderId { get; set; }
    public int? BillId { get; set; }
    public int BranchId { get; set; }
    public int? ReasonId { get; set; }
    public string? ReasonName { get; set; }
    public string? Number { get; set; }
    public DateTime Date { get; set; }
    public string? Observation { get; set; }

    public List<CreatePurchaseReturnDetailDto> Details { get; set; } = [];
}
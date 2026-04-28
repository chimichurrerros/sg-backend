using System;
using System.Collections.Generic;
using BackEnd.Models;

namespace BackEnd.DTOs.Requests.SalesOrder;

public class CreateSalesOrderRequestDto
{
    public int CustomerId { get; set; }
    public SalesOrderStateEnum SalesOrderState { get; set; }
    public DateTime? Date { get; set; }
    public BillTypeEnum? BillType { get; set; }
    public bool? IsCredit { get; set; }

    public int AccountId { get; set; }
    public int MovementType { get; set; }
    public int BranchId { get; set; }

    public List<CreateSalesOrderDetailRequestDto> Details { get; set; } = new();
}

public class CreateSalesOrderDetailRequestDto
{
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
}

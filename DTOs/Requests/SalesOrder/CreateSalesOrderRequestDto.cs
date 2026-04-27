using System;
using System.Collections.Generic;
using BackEnd.Models;

namespace BackEnd.DTOs.Requests.SalesOrder;

public class CreateSalesOrderRequestDto
{
    // SalesOrder properties
    public int CustomerId { get; set; }
    public string Number { get; set; } = null!;
    public SalesOrderStateEnum SalesOrderState { get; set; }

    // Bill properties
    public string BillNumber { get; set; } = null!;
    public BillStateEnum BillState { get; set; }

    // Payment and Stock
    public int AccountId { get; set; }
    public int MovementType { get; set; }
    public int BranchId { get; set; }

    // Details
    public List<CreateSalesOrderDetailRequestDto> Details { get; set; } = new();
}

public class CreateSalesOrderDetailRequestDto
{
    public int ProductId { get; set; }
    public decimal Quantity { get; set; }
    //public decimal Price { get; set; }
    //public decimal TaxRate { get; set; }
}

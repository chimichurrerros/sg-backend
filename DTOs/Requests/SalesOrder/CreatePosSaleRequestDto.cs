using System;
using System.Collections.Generic;
using BackEnd.Models;

namespace BackEnd.DTOs.Requests.SalesOrder;

public class CreatePosSaleRequestDto
{
    public PosSaleCustomerRequestDto? Customer { get; set; } = new();
    public PosSaleDataRequestDto Sale { get; set; } = new();
    public PosSalePayRequestDto Pay { get; set; } = new();
    public List<PosSaleProductRequestDto> Products { get; set; } = new();
    public PosSaleTotalsRequestDto Totals { get; set; } = new();
}

public class PosSaleCustomerRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Ruc { get; set; } = string.Empty;
}

public class PosSaleDataRequestDto
{
    public BillTypeEnum? Bill { get; set; }
    public DateTime? Date { get; set; }
    public int? CashierNumber { get; set; }
    public int? BranchId { get; set; }
    public int? AccountId { get; set; }
    public int? MovementType { get; set; }
}

public class PosSalePayRequestDto
{
    public PosPaymentMethod Method { get; set; }
    public PosSaleCondition Condition { get; set; }
}

public class PosSaleProductRequestDto
{
    public int? ProductId { get; set; }
    public string? Barcode { get; set; }
    public decimal Quantity { get; set; }
}

public class PosSaleTotalsRequestDto
{
    public decimal Subtotal { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
    public decimal Amount { get; set; }
    public decimal Change { get; set; }
    public decimal ImportValue {get; set;}
}

public enum PosPaymentMethod
{
    Cash = 1,
    Card = 2,
    Transfer = 3,
    Check = 4,
    Other = 99
}

public enum PosSaleCondition
{
    Cash = 1,
    Credit = 2
}

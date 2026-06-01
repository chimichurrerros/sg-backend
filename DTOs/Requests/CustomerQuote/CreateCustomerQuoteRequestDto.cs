using BackEnd.DTOs.Requests.SalesOrder;
using BackEnd.Models;

namespace BackEnd.DTOs.Requests.CustomerQuote;

public class CreateCustomerQuoteRequestDto
{
    public CustomerQuoteCustomerRequestDto? Customer { get; set; } = new();
    public CustomerQuoteDataRequestDto Sale { get; set; } = new();
    public CustomerQuotePayRequestDto Pay { get; set; } = new();
    public List<CustomerQuoteProductRequestDto> Products { get; set; } = new();
    public CustomerQuoteTotalsRequestDto Totals { get; set; } = new();
}

public class UpdateCustomerQuoteRequestDto
{
    public CustomerQuoteCustomerRequestDto? Customer { get; set; } = new();
    public CustomerQuoteDataRequestDto Sale { get; set; } = new();
    public CustomerQuotePayRequestDto Pay { get; set; } = new();
    public List<CustomerQuoteProductRequestDto> Products { get; set; } = new();
    public CustomerQuoteTotalsRequestDto Totals { get; set; } = new();
}

public class CustomerQuoteCustomerRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string Ruc { get; set; } = string.Empty;
    public DateOnly? BirthDate { get; set; }
    public string? Email { get; set; }
}

public class CustomerQuoteDataRequestDto
{
    public BillTypeEnum? Bill { get; set; }
    public DateTime? Date { get; set; }
    public int? CashierNumber { get; set; }
    public int? BranchId { get; set; }
    public int? AccountId { get; set; }
    public int? MovementType { get; set; }
}

public class CustomerQuotePayRequestDto
{
    public PosPaymentMethod Method { get; set; }
    public PosSaleCondition Condition { get; set; }
}

public class CustomerQuoteProductRequestDto
{
    public int? ProductId { get; set; }
    public string? Barcode { get; set; }
    public decimal Quantity { get; set; }
    public decimal Price { get; set; }
}

public class CustomerQuoteTotalsRequestDto
{
    public decimal Subtotal { get; set; }
    public decimal Iva { get; set; }
    public decimal Total { get; set; }
    public decimal Amount { get; set; }
    public decimal Change { get; set; }
    public decimal ImportValue { get; set; }
}

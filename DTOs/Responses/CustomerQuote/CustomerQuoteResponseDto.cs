using BackEnd.Models;
using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.CustomerQuote;

public class CustomerQuoteDetailResponseDto
{
    public int Id { get; set; }

    public int ProductId { get; set; }

    public string? ProductName { get; set; }

    public decimal Quantity { get; set; }

    public decimal Price { get; set; }
}

public class CustomerQuoteResponseDto
{
    public int Id { get; set; }

    public int CustomerId { get; set; }

    public string? CustomerName { get; set; }

    public int UserId { get; set; }

    public string? UserName { get; set; }

    public DateTime Date { get; set; }

    public DateTime ExpirationDate => Date.AddDays(10);

    public decimal Total { get; set; }

    public decimal ImportValue { get; set; }

    public int BranchId { get; set; }

    public string? Number { get; set; }

    public PaymentMethodEnum? PaymentMethod { get; set; }

    public SaleConditionEnum? SaleCondition { get; set; }

    public BillTypeEnum? BillType { get; set; }

    public BackEnd.Models.CustomerQuote.QuoteStatus Status { get; set; }

    public List<CustomerQuoteDetailResponseDto> Details { get; set; } = [];

    public int? AssociatedSalesOrderId { get; set; }
}

public class CustomerQuoteWrapperDto
{
    public CustomerQuoteResponseDto CustomerQuote { get; set; } = null!;
}

public class ListCustomerQuotesWrapperDto
{
    public List<CustomerQuoteResponseDto> CustomerQuotes { get; set; } = [];

    public Pagination Pagination { get; set; } = null!;
}

public class CustomerQuoteDetailWrapperDto
{
    public CustomerQuoteDetailResponseDto CustomerQuoteDetail { get; set; } = null!;
}

public class ListCustomerQuoteDetailsWrapperDto
{
    public List<CustomerQuoteDetailResponseDto> CustomerQuoteDetails { get; set; } = [];
}

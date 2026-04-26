using BackEnd.Models;
using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.CustomerQuote;

/// <summary>
/// Represents a customer quote line returned by the API.
/// </summary>
public class CustomerQuoteDetailResponseDto
{
    /// <summary>
    /// Unique identifier of the quote detail line.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Identifier of the product included in the quote line.
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Optional product name for display purposes.
    /// </summary>
    public string? ProductName { get; set; }

    /// <summary>
    /// Quantity quoted for the product.
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Unit price quoted for the product.
    /// </summary>
    public decimal Price { get; set; }
}

/// <summary>
/// Represents a customer quote returned by the API.
/// </summary>
public class CustomerQuoteResponseDto
{
    /// <summary>
    /// Unique identifier of the quote.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Identifier of the customer associated with the quote.
    /// </summary>
    public int CustomerId { get; set; }

    /// <summary>
    /// Optional customer name for display purposes.
    /// </summary>
    public string? CustomerName { get; set; }

    /// <summary>
    /// Identifier of the user who created the quote.
    /// </summary>
    public int UserId { get; set; }

    /// <summary>
    /// Optional user name for display purposes.
    /// </summary>
    public string? UserName { get; set; }

    /// <summary>
    /// Creation date of the quote.
    /// </summary>
    public DateTime Date { get; set; }

    /// <summary>
    /// Total amount of the quote.
    /// </summary>
    public decimal Total { get; set; }

    /// <summary>
    /// Current status of the quote.
    /// </summary>
    public BackEnd.Models.CustomerQuote.QuoteStatus Status { get; set; }

    /// <summary>
    /// Collection of quote detail lines.
    /// </summary>
    public List<CustomerQuoteDetailResponseDto> Details { get; set; } = [];
}

/// <summary>
/// Wrapper for a single customer quote response.
/// </summary>
public class CustomerQuoteWrapperDto
{
    /// <summary>
    /// Single customer quote payload.
    /// </summary>
    public CustomerQuoteResponseDto CustomerQuote { get; set; } = null!;
}

/// <summary>
/// Wrapper for a paginated list of customer quotes.
/// </summary>
public class ListCustomerQuotesWrapperDto
{
    /// <summary>
    /// Collection of customer quotes.
    /// </summary>
    public List<CustomerQuoteResponseDto> CustomerQuotes { get; set; } = [];

    /// <summary>
    /// Pagination metadata for the current result set.
    /// </summary>
    public Pagination Pagination { get; set; } = null!;
}

/// <summary>
/// Wrapper for a single customer quote detail response.
/// </summary>
public class CustomerQuoteDetailWrapperDto
{
    /// <summary>
    /// Single customer quote detail payload.
    /// </summary>
    public CustomerQuoteDetailResponseDto CustomerQuoteDetail { get; set; } = null!;
}

/// <summary>
/// Wrapper for a list of customer quote detail lines.
/// </summary>
public class ListCustomerQuoteDetailsWrapperDto
{
    /// <summary>
    /// Collection of customer quote detail lines.
    /// </summary>
    public List<CustomerQuoteDetailResponseDto> CustomerQuoteDetails { get; set; } = [];
}

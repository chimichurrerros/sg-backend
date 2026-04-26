using System.ComponentModel.DataAnnotations;

namespace BackEnd.DTOs.Requests.CustomerQuote;

/// <summary>
/// DTO for a quote detail line used when creating or updating a customer quote.
/// </summary>
public class CustomerQuoteDetailRequestDto
{
    /// <summary>
    /// Identifier of the product to include in the quote.
    /// </summary>
    [Required]
    public int ProductId { get; set; }

    /// <summary>
    /// Quantity requested for the product.
    /// </summary>
    [Range(typeof(decimal), "0.01", "79228162514264337593543950335")]
    public decimal Quantity { get; set; }

    /// <summary>
    /// Unit price assigned to the product in the quote.
    /// </summary>
    [Range(typeof(decimal), "0.00", "79228162514264337593543950335")]
    public decimal Price { get; set; }
}

/// <summary>
/// DTO for creating a new customer quote.
/// </summary>
public class CreateCustomerQuoteRequestDto
{
    /// <summary>
    /// Identifier of the customer receiving the quote.
    /// </summary>
    [Required]
    public int CustomerId { get; set; }

    /// <summary>
    /// Identifier of the user creating the quote.
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// Quote detail lines included in the quote.
    /// </summary>
    [MinLength(1)]
    public List<CustomerQuoteDetailRequestDto> Details { get; set; } = [];
}

/// <summary>
/// DTO for updating an existing customer quote.
/// </summary>
public class UpdateCustomerQuoteRequestDto
{
    /// <summary>
    /// Identifier of the customer receiving the quote.
    /// </summary>
    [Required]
    public int CustomerId { get; set; }

    /// <summary>
    /// Identifier of the user responsible for the quote update.
    /// </summary>
    [Required]
    public int UserId { get; set; }

    /// <summary>
    /// New quote detail lines that replace the current quote lines.
    /// </summary>
    [MinLength(1)]
    public List<CustomerQuoteDetailRequestDto> Details { get; set; } = [];
}

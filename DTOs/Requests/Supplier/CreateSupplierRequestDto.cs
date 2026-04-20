using System.ComponentModel.DataAnnotations;
using BackEnd.Constants.Errors;

namespace BackEnd.DTOs.Requests.Supplier;

/// <summary>
/// DTO for creating a new supplier. Suppliers are always associated with a LegalPerson entity.
/// </summary>
public class CreateSupplierRequestDto
{
    /// <summary>
    /// Unique document number (e.g., RUC, VAT ID) for the supplier's legal entity.
    /// </summary>
    [Required(ErrorMessage = SupplierError.DocumentNumberRequired)]
    public string DocumentNumber { get; set; } = null!;

    /// <summary>
    /// Business/company name of the legal person.
    /// </summary>
    [Required(ErrorMessage = SupplierError.BusinessNameRequired)]
    public string BusinessName { get; set; } = null!;

    /// <summary>
    /// Optional fantasy name or trade name used by the legal person.
    /// </summary>
    public string? FantasyName { get; set; }

    /// <summary>
    /// Email address for contacting the supplier.
    /// </summary>
    [EmailAddress(ErrorMessage = EmailError.InvalidEmail)]
    public string? Email { get; set; }

    /// <summary>
    /// Phone number for contacting the supplier.
    /// </summary>
    public string? Phone { get; set; }

    /// <summary>
    /// Physical address of the supplier's business.
    /// </summary>
    public string? Address { get; set; }

    /// <summary>
    /// Indicates whether the supplier is currently active.
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// List of product category IDs that this supplier can provide.
    /// </summary>
    public List<int> ProductCategoryIds { get; set; } = [];
}

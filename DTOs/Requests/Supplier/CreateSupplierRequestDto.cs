using System.ComponentModel.DataAnnotations;
using BackEnd.Constants.Errors;
using BackEnd.Models;

namespace BackEnd.DTOs.Requests.Supplier;

public class CreateSupplierRequestDto
{
    [Required(ErrorMessage = SupplierError.EntityTypeRequired)]
    public EntityPersonType? EntityType { get; set; }

    [Required(ErrorMessage = SupplierError.DocumentNumberRequired)]
    public string DocumentNumber { get; set; } = null!;

    [EmailAddress(ErrorMessage = EmailError.InvalidEmail)]
    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Address { get; set; }

    public string? BusinessName { get; set; }

    public string? FantasyName { get; set; }

    public string? FirstName { get; set; }

    public string? LastName { get; set; }

    public int? GenderId { get; set; }

    public int? MaritalStatusId { get; set; }

    public DateOnly? BirthDate { get; set; }

    public bool IsActive { get; set; } = true;

    public List<int> ProductCategoryIds { get; set; } = [];
}

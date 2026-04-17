using System.ComponentModel.DataAnnotations;
using BackEnd.Constants.Errors;
using BackEnd.Models;

namespace BackEnd.DTOs.Requests.Supplier;

public class PatchSupplierRequestDto
{
    public EntityPersonType? EntityType { get; set; }

    public string? DocumentNumber { get; set; }

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

    public bool? IsActive { get; set; }

    public List<int>? ProductCategoryIds { get; set; }
}

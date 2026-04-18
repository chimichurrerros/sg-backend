using System.ComponentModel.DataAnnotations;
using BackEnd.Constants.Errors;

namespace BackEnd.DTOs.Requests.Employee;

public class UpdateEmployeeRequestDto
{
    // Employee Info
    [Required(ErrorMessage = EmployeeError.FileNumberRequired)]
    public string FileNumber { get; set; } = null!;

    [Required(ErrorMessage = EmployeeError.HireDateRequired)]
    public DateOnly HireDate { get; set; }

    public int AreaId { get; set; }
    public int? InmediatlyBossId { get; set; }

    // PhysicalPerson Info
    [Required(ErrorMessage = EmployeeError.FirstNameRequired)]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = EmployeeError.LastNameRequired)]
    public string Lastname { get; set; } = null!;

    public DateOnly BirthDate { get; set; }
    public int GenderId { get; set; }
    public int MaritalStatusId { get; set; }

    // Base Entity Info
    [Required(ErrorMessage = EmployeeError.DocumentNumberRequired)]
    public string DocumentNumber { get; set; } = null!;

    [EmailAddress(ErrorMessage = EmailError.InvalidEmail)]
    public string? Email { get; set; }

    public string? Phone { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
}

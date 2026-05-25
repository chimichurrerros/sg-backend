using System.ComponentModel.DataAnnotations;
using BackEnd.Constants.Errors;
using BackEnd.Models;

namespace BackEnd.DTOs.Requests.Employee;

public class CreateEmployeeRequestDto
{
    // Employee Info
    [Required(ErrorMessage = EmployeeError.FileNumberRequired)]
    public string FileNumber { get; set; } = null!;

    [Required(ErrorMessage = EmployeeError.HireDateRequired)]
    public DateOnly HireDate { get; set; }

    public int AreaId { get; set; }
    public int? BranchId { get; set; }
    public int? InmediatlyBossId { get; set; }

    [Required(ErrorMessage = EmployeeError.InvalidPosition)]
    public int PositionId { get; set; }

    [Required(ErrorMessage = EmployeeError.InvalidSchedule)]
    public int ScheduleId { get; set; }

    [Required(ErrorMessage = EmployeeError.BasicSalaryRequired)]
    public decimal BasicSalary { get; set; }

    [Required(ErrorMessage = EmployeeError.PositionStartDateRequired)]
    public DateOnly PositionStartDate { get; set; }

    // PhysicalPerson Info
    [Required(ErrorMessage = EmployeeError.FirstNameRequired)]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = EmployeeError.LastNameRequired)]
    public string Lastname { get; set; } = null!;

    public DateOnly BirthDate { get; set; }
    public int GenderId { get; set; }
    public BackEnd.Models.Employee.MaritalStatusEnum MaritalStatus { get; set; }

    // Base Entity Info
    [Required(ErrorMessage = EmployeeError.DocumentNumberRequired)]
    public string DocumentNumber { get; set; } = null!;

    [EmailAddress(ErrorMessage = EmailError.InvalidEmail)]
    public string? Email { get; set; }

    public string? Phone { get; set; }
    public string? Address { get; set; }
    public bool IsActive { get; set; } = true;
}

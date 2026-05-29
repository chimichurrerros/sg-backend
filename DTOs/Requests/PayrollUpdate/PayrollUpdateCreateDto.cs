using System.ComponentModel.DataAnnotations;
using BackEnd.Constants.Errors;

namespace BackEnd.DTOs.Requests.PayrollUpdate;

public class PayrollUpdateCreateDto
{
    [Required(ErrorMessage = PayrollUpdateError.NameRequired)]
    public string Name { get; set; } = null!;

    public int PayrollTypeId { get; set; }

    public int FormulaTypeId { get; set; }

    public string? Formula { get; set; }

    public bool IpsDeductible { get; set; }
}
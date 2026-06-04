using System.ComponentModel.DataAnnotations;
using BackEnd.Constants.Errors;
using BackEnd.Models;

namespace BackEnd.DTOs.Requests.Employee;

public class UpdateEmployeeRelationRequestDto
{
    [Required(ErrorMessage = EmployeeError.FamilyRelationTypeInvalid)]
    public BackEnd.Models.EmployeeRelation.RelationTypeEnum RelationType { get; set; }

    [Required(ErrorMessage = EmployeeError.FamilyNameRequired)]
    public string Name { get; set; } = null!;

    [Required(ErrorMessage = EmployeeError.FamilyLastnameRequired)]
    public string Lastname { get; set; } = null!;

    [Required(ErrorMessage = EmployeeError.FamilyDocumentRequired)]
    public string DocumentNumber { get; set; } = null!;

    [Required(ErrorMessage = EmployeeError.FamilyBirthDateRequired)]
    public DateOnly BirthDate { get; set; }
}

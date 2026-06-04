namespace BackEnd.Constants.Errors;

public static class EmployeeError
{
    public const string DocumentNumberRequired = "El documento es obligatorio";
    public const string DocumentNumberAlreadyExists = "Ya existe un empleado con ese documento";
    public const string EmployeeNotFound = "No se encontró el empleado solicitado";
    public const string FirstNameRequired = "El nombre es obligatorio";
    public const string LastNameRequired = "El apellido es obligatorio";
    public const string FileNumberRequired = "El número de legajo es obligatorio";
    public const string HireDateRequired = "La fecha de contratación es obligatoria";
    public const string InvalidArea = "El departamento / área seleccionada no existe";
    public const string InvalidGender = "El género seleccionado no existe";
    public const string InvalidMaritalStatus = "El estado civil seleccionado es inválido";
    public const string InvalidInmediatlyBoss = "El jefe inmediato seleccionado no existe";
    public const string InvalidBranch = "La sucursal seleccionada no existe";
    public const string InvalidPosition = "El cargo seleccionado no existe";
    public const string InvalidSchedule = "El horario seleccionado no existe";
    public const string BasicSalaryRequired = "El salario base es obligatorio";
    public const string BasicSalaryMustBeGreaterThanZero = "El salario base debe ser mayor a 0";
    public const string PositionStartDateRequired = "La fecha de inicio del cargo es obligatoria";
    public const string PositionHistoryNotFound = "No se encontró el historial de cargo solicitado";
    public const string PositionStartDateBeforeHireDate = "La fecha de inicio del cargo no puede ser anterior al ingreso del empleado";
    public const string PositionStartDateMustBeAfterCurrent = "La fecha de inicio del nuevo cargo debe ser posterior al cargo actual";
    public const string PositionEndDateInvalid = "La fecha de fin del cargo debe ser posterior o igual a la fecha de inicio";
    public const string FamilyRelationNotFound = "No se encontró la relación familiar solicitada";
    public const string FamilyRelationTypeInvalid = "El tipo de relación familiar es inválido";
    public const string FamilyNameRequired = "El nombre del familiar es obligatorio";
    public const string FamilyLastnameRequired = "El apellido del familiar es obligatorio";
    public const string FamilyDocumentRequired = "El documento del familiar es obligatorio";
    public const string FamilyDocumentAlreadyExists = "Ya existe un familiar con ese documento para este empleado";
    public const string FamilyBirthDateRequired = "La fecha de nacimiento del familiar es obligatoria";
    public const string EmployeeAlreadyInactive = "El empleado ya se encuentra inactivo";
    public const string PositionHistoryNotEditable = "Solo se puede editar el historial de cargo más reciente";
    public const string PositionHistoryNotDeletable = "Solo se puede eliminar el historial de cargo más reciente";
}

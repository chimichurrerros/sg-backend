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
    public const string InvalidMaritalStatus = "El estado civil seleccionado no existe";
    public const string InvalidInmediatlyBoss = "El jefe inmediato seleccionado no existe";
}

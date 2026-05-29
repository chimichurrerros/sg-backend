namespace BackEnd.Constants.Errors;

public static class PayrollUpdateError
{
    public const string NameRequired = "El nombre del concepto de nómina es obligatorio";
    public const string FormulaRequired = "La fórmula del concepto de nómina es obligatoria";
    public const string InvalidPayrollType = "El tipo de nómina seleccionado no existe";
    public const string InvalidFormulaType = "El tipo de fórmula seleccionado no existe";
    public const string FixedFormulaMustBeNumeric = "La fórmula fija debe ser un número válido";
    public const string CalculatedFormulaIsInvalid = "La fórmula calculada contiene una expresión inválida";
    public const string PayrollUpdateNotFound = "No se encontró el concepto de nómina solicitado";
}
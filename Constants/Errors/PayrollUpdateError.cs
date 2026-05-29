namespace BackEnd.Constants.Errors;

public static class PayrollUpdateError
{
    public const string NameRequired = "The payroll concept name is required";
    public const string FormulaRequiredForCalculated = "The payroll concept formula is required for calculated concepts";
    public const string InvalidPayrollType = "The selected payroll type does not exist";
    public const string InvalidFormulaType = "The selected formula type does not exist";
    public const string CalculatedFormulaIsInvalid = "The calculated formula contains an invalid expression";
    public const string PayrollUpdateNotFound = "The requested payroll concept was not found";
}
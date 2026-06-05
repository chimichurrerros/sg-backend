namespace BackEnd.Constants.Errors;

public static class PayrollManualDetailError
{
    public const string PayrollUpdateMustBeFixed = "Manual payroll details are only allowed for fixed concepts";
    public const string ManualDetailNotFound = "The requested manual payroll detail was not found";
}
namespace BackEnd.Constants.Errors;

public static class PayrollProcessError
{
    public const string PayrollProcessNotFound = "The requested payroll process was not found";
    public const string PayrollProcessStatusNotFound = "The payroll status catalog is not configured correctly";
    public const string PayrollProcessMustBeOpen = "The payroll process must be open before it can be calculated";
    public const string PayrollProcessCannotBeModified = "The payroll process cannot be modified because it is in a final state";
    public const string ManualAmountRequired = "A manual amount is required for the fixed payroll concept";
    public const string MissingPositionAssignment = "The employee does not have an active position assignment for the selected payroll period";
}
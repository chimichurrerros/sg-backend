namespace BackEnd.Constants.Errors;

public static class AccountingError
{
    public const string ProcessExpiredOrNotExists = "El período del proceso contable ha vencido o no existe.";
    public const string CurrentProcessExpired = "El período del proceso contable actual ha vencido.";
    public const string NewProcessExpiredOrNotExists = "El período del nuevo proceso contable ha vencido o no existe.";
    public const string CannotDeleteProcessExpired = "No se puede eliminar porque el período del proceso contable ha vencido.";
    public const string EntryNotBalanced = "El asiento contable no está balanceado. Debe ({0}) != Haber ({1}).";
    public const string EntryHasNoDetails = "El asiento contable debe tener al menos un detalle.";
}

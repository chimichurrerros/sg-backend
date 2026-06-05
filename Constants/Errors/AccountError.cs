namespace BackEnd.Constants.Errors;

public static class AccountError
{
    public const string AccountNotFound = "La cuenta bancaria seleccionada no existe.";
    public const string NotEnoughFunds = "Saldo insuficiente para realizar este movimiento.";
    public const string InvalidAmount = "El monto debe ser mayor a cero.";
}


namespace BackEnd.Constants.Errors;

public static class CustomerError
{
    public const string NameRequired = "El nombre es obligatorio";
    public const string RucRequired = "El RUC es obligatorio";
    public const string RucAlreadyExists = "Ya existe un cliente con ese RUC";
    public const string CustomerNotFound = "No se encontró el cliente solicitado";
}

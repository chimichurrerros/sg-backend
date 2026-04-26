namespace BackEnd.Constants.Errors;

public static class CustomerQuoteError
{
    public const string CustomerQuoteNotFound = "No se encontro el presupuesto solicitado";
    public const string CustomerIdRequired = "El cliente es obligatorio";
    public const string UserIdRequired = "El usuario es obligatorio";
    public const string DetailsRequired = "El presupuesto debe tener al menos un detalle";
    public const string InvalidDetailQuantity = "La cantidad debe ser mayor a cero";
    public const string InvalidDetailPrice = "El precio no puede ser negativo";
    public const string CustomerNotFound = "No se encontro el cliente indicado";
    public const string UserNotFound = "No se encontro el usuario indicado";
    public const string InvalidProducts = "Uno o mas productos no existen";
    public const string ExistingOpenQuote = "El cliente ya tiene un presupuesto vigente";
    public const string QuoteExpired = "El presupuesto esta vencido y no puede modificarse";
}
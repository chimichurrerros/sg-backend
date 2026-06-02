namespace BackEnd.Constants.Errors;

public static class StockError
{
    public const string QuantityMustBeGreaterThanZero = "La cantidad debe ser mayor a cero.";
    public const string InsufficientStock = "Stock insuficiente para el producto '{0}'. Disponible: {1}, Requerido: {2}.";
}
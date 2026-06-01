namespace BackEnd.Constants.Errors;

public static class SalesReturnError
{
    public const string ProcessFailed = "Error al procesar la devolución.";
    public const string BillNotFound = "Factura no encontrada.";
    public const string SalesOrderNotFound = "Orden de venta no encontrada.";
    public const string DetailsRequired = "Debe especificar al menos un producto.";
    public const string ReturnPeriodExpired = "El plazo de 48 horas para devoluciones ha expirado.";
    public const string ProductNotInSale = "El producto no pertenece a la venta original.";
    public const string QuantityExceedsSold = "La cantidad devuelta supera la cantidad facturada.";
    public const string NotFound = "Devolución no encontrada.";
}

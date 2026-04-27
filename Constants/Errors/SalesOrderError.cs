namespace BackEnd.Constants.Errors;

public static class SalesOrderError
{
    public const string ProductNotFound = "No se encontró uno de los productos de la venta.";
    public const string DetailsRequired = "La venta debe incluir al menos un detalle.";
    public const string CustomerNotFound = "No se encontró el cliente solicitado.";
    public const string StockUpdateFailed = "No se pudo actualizar el stock.";
    public const string BillCreateFailed = "No se pudo crear la factura.";
    public const string BillDetailCreateFailed = "No se pudo crear el detalle de factura.";
    public const string AccountNotFound = "No se encontró la cuenta bancaria solicitada.";
    public const string ProcessFailed = "No se pudo procesar la venta.";
    public const string NotFound = "No se encontró el pedido de venta solicitado.";
    // Sufijo que puede agregarse al final de un mensaje de error para indicar el id
    // de la entidad relacionada con el fallo. Use string.Format(SalesOrderError.IdSuffix, id)
    // cuando sea necesario anexar el identificador.
    public const string IdSuffix = " ID: {0}";
}
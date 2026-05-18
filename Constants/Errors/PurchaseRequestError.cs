namespace BackEnd.Constants.Errors;

public static class PurchaseRequestError
{
    public const string NotFound = "El pedido de compra no existe.";
    public const string ProcessFailed = "Ocurrió un error al procesar el pedido de compra.";
    public const string DetailsRequired = "El pedido de compra debe contener al menos un producto.";
    public const string ProductNotFound = "Uno de los productos especificados no fue encontrado.";
}

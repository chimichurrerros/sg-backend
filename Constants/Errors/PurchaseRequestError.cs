namespace BackEnd.Constants.Errors;

public static class PurchaseRequestError
{
    public const string NotFound = "El pedido de compra no existe.";
    public const string ProcessFailed = "Ocurrió un error al procesar el pedido de compra.";
    public const string DetailsRequired = "El pedido de compra debe contener al menos un producto.";
    public const string ProductNotFound = "Uno de los productos especificados no fue encontrado.";
    public const string ProductIsService = "El producto especificado es un servicio y no puede ser solicitado.";
    public const string BranchRequired = "El pedido de compra debe pertenecer a una sucursal.";
    public const string BranchNotFound = "La sucursal especificada no fue encontrada.";
}

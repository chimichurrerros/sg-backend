namespace BackEnd.Constants.Errors;

public static class PurchaseReceiptError
{
    public const string NotFound = "La recepción de orden de compra no fue encontrada.";
    public const string ProcessFailed = "Ocurrió un error al procesar la recepción.";
    public const string DetailsRequired = "La recepción debe contener al menos un producto.";
    public const string ProductNotFound = "Uno de los productos especificados no fue encontrado en la base de datos.";
    public const string PurchaseOrderNotFound = "La orden de compra especificada no existe.";
    public const string PurchaseOrderDetailNotFound = "El producto no forma parte de la orden de compra especificada.";
    public const string QuantityExceeded = "La cantidad a recibir supera la cantidad pendiente en la orden de compra.";
    public const string PaymentNotConfirmed = "No puede recibir sin haber procesado el pago";
    public const string SupplierMismatch = "El proveedor especificado no coincide con el proveedor de la orden de compra.";
    public const string ReceiptNotFound = "La recepción no fue encontrada.";
    public const string NumberAlreadyExists = "El número de recepción ya existe.";
}

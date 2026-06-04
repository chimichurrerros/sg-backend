namespace BackEnd.Constants.Errors;

public static class PurchaseReturnError
{
    public const string NotFound = "La nota de devolución no fue encontrada.";
    public const string ProcessFailed = "Ocurrió un error al procesar la nota de devolución.";
    public const string DetailsRequired = "La nota de devolución debe contener al menos un producto.";
    public const string PurchaseOrderNotFound = "La orden de compra especificada no existe.";
    public const string PurchaseOrderDetailNotFound = "El producto no forma parte de la orden de compra especificada.";
    public const string QuantityExceeded = "La cantidad a devolver supera la cantidad disponible para devolver.";
    public const string ReasonRequired = "Debe seleccionar o crear un motivo de devolución.";
    public const string ReasonNotFound = "El motivo de devolución especificado no existe.";
    public const string BillNotFound = "La factura especificada no existe.";
    public const string BranchNotFound = "La sucursal especificada no existe.";
    public const string PurchaseOrderIdMismatch  = "La combinación de factura y nota de devolución no existe";
}
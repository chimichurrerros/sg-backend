namespace BackEnd.Constants.Errors;

public static class PurchaseOrderError
{
    public const string PurchaseOrderNotFound = "No se encontro la orden de compra";
    public const string PurchaseRequestRequired = "El pedido de compra es obligatorio";
    public const string StateRequired = "El estado es obligatorio";
    public const string SupplierRequired = "El proveedor es obligatorio";
    public const string DetailsRequired = "La orden de compra debe tener al menos un detalle";
    public const string PurchaseRequestNotFound = "No se encontro el pedido de compra indicado";
    public const string InvalidProducts = "Uno o mas productos no pertenecen al pedido de compra o no tienen cotizacion valida";
    public const string InvalidSupplierQuoteDetail = "La cotizacion del detalle no pertenece al producto ni al pedido de compra";
    public const string InvalidQuantity = "La cantidad debe ser mayor a cero";
    public const string InvalidState = "No se encontro el estado indicado";
    public const string InvalidSupplier = "No se encontro el proveedor indicado";
}

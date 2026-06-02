namespace BackEnd.Constants.Errors;

public static class SupplierQuoteError
{
    public const string SupplierQuoteNotFound = "No se encontro la cotizacion del proveedor";
    public const string SupplierIdRequired = "El proveedor es obligatorio";
    public const string PurchaseRequestIdRequired = "El pedido de compra es obligatorio";
    public const string DetailsRequired = "La cotizacion debe tener al menos un detalle";
    public const string InvalidDetailQuantity = "La cantidad disponible debe ser mayor o igual a cero";
    public const string InvalidDetailPrice = "El precio no puede ser negativo";
    public const string SupplierNotFound = "No se encontro el proveedor indicado";
    public const string PurchaseRequestNotFound = "No se encontro el pedido de compra indicado";
    public const string RequestForQuotationIdRequired = "La solicitud de cotizacion es obligatoria";
    public const string RequestForQuotationNotFound = "No se encontro la solicitud de cotizacion indicada";
    public const string RequestForQuotationMismatch = "La solicitud de cotizacion no corresponde al proveedor o pedido de compra indicados";
    public const string InvalidProducts = "Uno o mas productos no existen o no pertenecen a la solicitud de cotizacion del proveedor";
    public const string QuantityExceedsRequested = "La cantidad disponible no puede superar la cantidad solicitada en la solicitud de cotizacion";
    public const string DuplicateQuote = "Ya existe una cotizacion para esta solicitud de cotizacion";
}

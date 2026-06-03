namespace BackEnd.Constants.Errors;

public static class RequestForQuotationError
{
    public const string InsufficientSuppliers = "Se requiere al menos 3 proveedores para generar las solicitudes de cotización.";
    public const string InvalidSuppliers = "Uno o más proveedores seleccionados no son válidos para los productos solicitados.";
    public const string SuppliersRequired = "Debe seleccionar al menos un proveedor.";
    public const string NoEligibleProducts = "Ninguno de los productos seleccionados puede ser asignado al proveedor especificado.";
    public const string SupplierNoCategoryMatch = "El proveedor seleccionado no cubre ninguna categoría de los productos solicitados.";
    public const string NotFound = "La solicitud de cotización no existe.";
    public const string ProcessFailed = "Ocurrió un error al obtener las solicitudes de cotización.";
}

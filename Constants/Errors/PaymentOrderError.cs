namespace BackEnd.Constants.Errors;

public static class PaymentOrderError
{
    public const string PurchaseOrderNotFound = "Orden de compra no encontrada";
    public const string DetailsRequired = "Los detalles del pago son requeridos";
    public const string SupplierNotFound = "Proveedor no encontrado";
    public const string ProcessFailed = "Error al procesar el pago";
    public const string InvalidAmount = "Monto invalido";
    public const string PurchaseOrderMustBeConfirmed = "La orden de compra debe estar confirmada";
    public const string PaymentOrderNotFound = "Orden de pago no encontrada";
    public const string PaymentAlreadyProcessed = "La orden de pago ya fue procesada";
    public const string PendingStateNotFound = "No se encontro el estado Pending para ordenes de pago";
    public const string ProcessedStateNotFound = "No se encontro el estado Processed para ordenes de pago";
    public const string BankAccountRequired = "La cuenta bancaria es obligatoria.";
    public const string InvalidPaymentMethod = "El metodo de pago especificado no es valido.";
    public const string CreditNoteNotFound = "Una de las notas de credito especificadas no fue encontrada.";
    public const string CreditNoteNotPurchaseReturn = "La nota de credito debe ser de tipo devolucion de compra.";
    public const string CreditNoteAmountExceedsTotal = "El monto total de las notas de credito supera el monto del pago.";
}
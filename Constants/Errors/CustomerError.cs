namespace BackEnd.Constants.Errors;

public static class CustomerError
{
    public const string DocumentNumberRequired = "El documento es obligatorio";
    public const string DocumentNumberAlreadyExists = "Ya existe un cliente con ese documento";
    public const string BusinessNameRequired = "La razón social es obligatoria para persona jurídica";
    public const string CustomerNotFound = "No se encontró el cliente solicitado";
    public const string InvalidTaxCondition = "La condición frente al IVA (TaxCondition) es inválida o no existe";
}

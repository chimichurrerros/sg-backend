namespace BackEnd.Constants.Errors;

public static class SupplierError
{
    public const string EntityTypeRequired = "El tipo de entidad es obligatorio";
    public const string InvalidEntityType = "El tipo de entidad es inválido";
    public const string DocumentNumberRequired = "El documento es obligatorio";
    public const string DocumentNumberAlreadyExists = "Ya existe una entidad con ese documento";
    public const string BusinessNameRequired = "La razón social es obligatoria para persona jurídica";
    public const string FirstNameRequired = "El nombre es obligatorio para persona física";
    public const string LastNameRequired = "El apellido es obligatorio para persona física";
    public const string GenderRequired = "El género es obligatorio para persona física";
    public const string MaritalStatusRequired = "El estado civil es obligatorio para persona física";
    public const string BirthDateRequired = "La fecha de nacimiento es obligatoria para persona física";
    public const string InvalidProductCategories = "Una o más categorías de producto no existen";
    public const string EntityTypeNotConfigured = "No existe configuración para el tipo de entidad seleccionado";
    public const string SupplierNotFound = "No se encontró el proveedor solicitado";
}

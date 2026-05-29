using BackEnd.DTOs.Responses.PayrollVariable;

namespace BackEnd.Constants.Payroll;

public static class PayrollVariableCatalog
{
    private static readonly IReadOnlyList<PayrollVariableResponseDto> Variables = new[]
    {
        new PayrollVariableResponseDto
        {
            Code = "SalarioBase",
            Description = "Salario estipulado en el contrato del empleado."
        },
        new PayrollVariableResponseDto
        {
            Code = "JornalDiario",
            Description = "SalarioBase / 30."
        },
        new PayrollVariableResponseDto
        {
            Code = "DiasTrabajados",
            Description = "Días asistidos cargados en las novedades del mes."
        },
        new PayrollVariableResponseDto
        {
            Code = "CantidadHijos",
            Description = "Cantidad de hijos menores de 18 años del empleado (obtenidos de su núcleo familiar)."
        },
        new PayrollVariableResponseDto
        {
            Code = "TotalDeducibleIPS",
            Description = "Variable interna acumulada de ingresos imponibles."
        }
    };

    public static IReadOnlyList<PayrollVariableResponseDto> GetAll() => Variables;
}
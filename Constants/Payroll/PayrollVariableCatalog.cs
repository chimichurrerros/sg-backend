using BackEnd.DTOs.Responses.PayrollVariable;

namespace BackEnd.Constants.Payroll;

public static class PayrollVariableCatalog
{
    private static readonly IReadOnlyList<PayrollVariableResponseDto> Variables = new[]
    {
        new PayrollVariableResponseDto
        {
            Code = "SalarioBase",
            Name = "SalarioBase",
            Description = "Salario mensual estipulado en el contrato del funcionario."
        },
        new PayrollVariableResponseDto
        {
            Code = "JornalDiario",
            Name = "JornalDiario",
            Description = "Salario base dividido 30 días. Es el costo de un día de trabajo."
        },
        new PayrollVariableResponseDto
        {
            Code = "DiasTrabajados",
            Name = "DiasTrabajados",
            Description = "Días efectivamente laborados en el mes (30 menos ausencias injustificadas)."
        },
        new PayrollVariableResponseDto
        {
            Code = "DiasAusencia",
            Name = "DiasAusencia",
            Description = "Cantidad de inasistencias injustificadas registradas en el periodo."
        },
        new PayrollVariableResponseDto
        {
            Code = "DiasTardanza",
            Name = "DiasTardanza",
            Description = "Cantidad de llegadas tardías injustificadas registradas en el periodo."
        },
        new PayrollVariableResponseDto
        {
            Code = "CantidadHijos",
            Name = "CantidadHijos",
            Description = "Número de hijos menores de 18 años asociados al núcleo familiar del empleado."
        },
        new PayrollVariableResponseDto
        {
            Code = "TotalDeducibleIPS",
            Name = "TotalDeducibleIPS",
            Description = "Suma total acumulada de todos los haberes del mes que tienen activado el parámetro Deducible de IPS."
        }
    };

    public static IReadOnlyList<PayrollVariableResponseDto> GetAll() => Variables;
}
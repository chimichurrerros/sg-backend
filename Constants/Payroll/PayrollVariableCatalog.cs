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
        },
        new PayrollVariableResponseDto
        {
<<<<<<< HEAD
            Code = "SueldoMinimo",
            Name = "SueldoMinimo",
            Description = "Salario mínimo legal vigente en Paraguay."
=======
            Code = "AniosAntiguedad",
            Name = "AniosAntiguedad",
            Description = "Años de servicio del empleado en la empresa desde su fecha de ingreso."
        },
        new PayrollVariableResponseDto
        {
            Code = "SueldoMinimo",
            Name = "SueldoMinimo",
            Description = "Salario mínimo legal vigente. Base de cálculo para subsidios y aportes (2.899.048 Gs.)."
        },
        new PayrollVariableResponseDto
        {
            Code = "ValorHoraOrdinaria",
            Name = "ValorHoraOrdinaria",
            Description = "Costo de una hora regular de trabajo (JornalDiario ÷ 8)."
        },
        new PayrollVariableResponseDto
        {
            Code = "HorasTardanza",
            Name = "HorasTardanza",
            Description = "Total de horas acumuladas por llegadas tardías en el periodo."
        },
        new PayrollVariableResponseDto
        {
            Code = "CantidadHoras50",
            Name = "CantidadHoras50",
            Description = "Horas extras trabajadas con recargo del 50%."
        },
        new PayrollVariableResponseDto
        {
            Code = "CantidadHoras100",
            Name = "CantidadHoras100",
            Description = "Horas extras trabajadas en feriados o domingos (recargo del 100%)."
>>>>>>> f2106ea2e1fd48c0df24fb0c460216e542820eaf
        }
    };

    public static IReadOnlyList<PayrollVariableResponseDto> GetAll() => Variables;
}
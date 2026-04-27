using System;
using System.Collections.Generic;

namespace BackEnd.Models;

public enum CheckType
{
    Day,
    Deferred
}

public enum CheckStatus
{
    Pending,
    Cashed,
    Voided
}

public partial class Check
{
    public int Id { get; set; }

    public string Number { get; set; } = null!;

    public DateTime EmisionDate { get; set; } // Fecha de emision

    // Cuando sea del tipo day availability date es igual a emision date
    public DateOnly AvailabilityDate { get; set; } // Fecha de disponibilidad

    public DateOnly? PaymentDate { get; set; } // Fecha de cobro

    // Cheque al dia vence en 30 dias
    // Diferido en 6meses
    public DateOnly? MaturityDate { get; set; } // Fecha de vencimiento

    public string IssuingBank { get; set; } = null!;

    public CheckType Type { get; set; }

    public string Receiver { get; set; } = null!;

    public decimal Amount { get; set; }

    public CheckStatus Status { get; set; }
}
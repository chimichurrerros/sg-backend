namespace BackEnd.DTOs.Requests.Checks;

using BackEnd.Models;
using System;
public class UpdateCheckStatusRequestDto
{
    public CheckStatusEnum Status { get; set; }
    public DateOnly? PaymentDate { get; set; }
}
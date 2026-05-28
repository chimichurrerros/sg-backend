using AutoMapper;
using BackEnd.Models;
using BackEnd.DTOs.Requests.Bank.BankMovement;
using BackEnd.DTOs.Responses.Bank.BankMovement;

public class BankMovementMapper : Profile
{
    public BankMovementMapper()
    {
        CreateMap<BankMovement, BankMovementRequestDto>();
        CreateMap<BankMovementRequestDto, BankMovement>();
        CreateMap<BankMovement, BankMovementResponseDto>();
    }
}

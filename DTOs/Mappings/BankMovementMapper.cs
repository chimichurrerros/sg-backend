using AutoMapper;
using BackEnd.Models;
using BackEnd.DTOs.Requests.Bank.BankMovement;
using BackEnd.DTOs.Responses.Bank.BankMovement;

public class BankMovementMapper : Profile
{
    public BankMovementMapper()
    {
        CreateMap<BankMovement, BankMovementResponseDto>()
            .ForMember(dest => dest.BankAccountId, opt => opt.MapFrom(src => src.AccountId));
        CreateMap<BankMovement, BankMovementRequestDto>();
        CreateMap<BankMovementRequestDto, BankMovement>();
        CreateMap<BankMovement, BankMovementResponseDto>();
    }
}

using BackEnd.DTOs.Requests.Bank;
using BackEnd.DTOs.Responses.Bank;
using BackEnd.Models;
using AutoMapper;

namespace BackEnd.DTOs.Mappings;

public class BankMapper : Profile
{
    public BankMapper()
    {
        CreateMap<BankRequestDto, Bank>();

        CreateMap<UpdateBankRequestDto, Bank>()
            .ForAllMembers(opt => opt.Condition((src, dest, srcVal) => srcVal != null));
            
        CreateMap<Bank, BankResponseDto>()
            .ForMember(dest => dest.Accounts, opt => opt.MapFrom(src => src.Accounts));

        CreateMap<Account, BankAccountResponseDto>();

        CreateMap<Bank, BankWrapperDto>()
            .ForMember(dest => dest.Bank, opt => opt.MapFrom(src => src));
    }
}

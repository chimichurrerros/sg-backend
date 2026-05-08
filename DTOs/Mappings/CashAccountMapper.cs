using BackEnd.DTOs.Requests.CashAccount;
using BackEnd.DTOs.Responses.CashAccount;
using BackEnd.Models;

namespace BackEnd.DTOs.Mappings;

public class CashAccountMapper : AutoMapper.Profile
{
    public CashAccountMapper()
    {
        CreateMap<CashAccount, CashAccountResponseDto>()
            .ForMember(dest => dest.BranchName,
                       opt => opt.MapFrom(src => src.Branch!.Name));

        CreateMap<CashAccount, CashAccountWrapperDto>()
            .ForMember(dest => dest.CashAccount,
                       opt => opt.MapFrom(src => src));

        CreateMap<CashAccountRequestDto, CashAccount>();
    }
}

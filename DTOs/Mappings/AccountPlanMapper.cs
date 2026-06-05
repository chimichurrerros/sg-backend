using BackEnd.DTOs.Requests.AccountPlan;
using BackEnd.DTOs.Responses.AccountPlan;
using BackEnd.Models;

namespace BackEnd.DTOs.Mappings;

public class AccountPlanMapper : AutoMapper.Profile
{
    public AccountPlanMapper()
    {
        CreateMap<AccountPlan, AccountPlanResponseDto>();

        CreateMap<AccountPlan, AccountPlanWrapperDto>()
            .ForMember(dest => dest.AccountPlan,
                       opt => opt.MapFrom(src => src));

        CreateMap<CreateAccountPlanRequestDto, AccountPlan>();
        CreateMap<UpdateAccountPlanRequestDto, AccountPlan>();
    }
}

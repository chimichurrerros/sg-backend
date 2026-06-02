using BackEnd.DTOs.Requests.AccountantProcess;
using BackEnd.DTOs.Responses.AccountantProcess;
using BackEnd.Models;

public class AccountantProcessMapper : AutoMapper.Profile
{
    public AccountantProcessMapper()
    {
        CreateMap<AccountantProcess, AccountantProcessResponseDto>();

        CreateMap<AccountantProcess, AccountantProcessWrapperDto>()
            .ForMember(dest => dest.AccountantProcess,
                       opt => opt.MapFrom(src => src));

        CreateMap<CreateAccountantProcessRequestDto, AccountantProcess>();

        CreateMap<UpdateAccountantProcessRequestDto, AccountantProcess>();
    }
}
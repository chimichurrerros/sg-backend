using BackEnd.DTOs.Responses.Branch;
using BackEnd.DTOs.Responses.Customer;
using BackEnd.Models;

namespace BackEnd.DTOs.Mappings;

public class BranchMapper : AutoMapper.Profile
{
    public BranchMapper()
    {
        CreateMap<Branch, BranchResponseDto>();

        CreateMap<Branch, BranchWrapperDto>()
            .ForMember(dest => dest.Branch,
                       opt => opt.MapFrom(src => src));
    }
}

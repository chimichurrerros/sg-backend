using BackEnd.DTOs.Requests.Branch;
using BackEnd.DTOs.Responses.Branch;
using BackEnd.Models;

namespace BackEnd.DTOs.Mappings;

public class BranchMapper : AutoMapper.Profile
{
    public BranchMapper()
    {
        CreateMap<BranchRequestDto, Branch>();
        CreateMap<Branch, BranchResponseDto>();

        CreateMap<Branch, BranchWrapperDto>()
            .ForMember(dest => dest.Branch,
                       opt => opt.MapFrom(src => src));
    }
}

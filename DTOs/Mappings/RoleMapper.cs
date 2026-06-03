using System.Linq;
using BackEnd.DTOs.Requests.Role;
using BackEnd.DTOs.Responses.Role;
using BackEnd.Models;

namespace BackEnd.DTOs.Mappings;

public class RoleMapper : AutoMapper.Profile
{
    public RoleMapper()
    {
        CreateMap<Role, RoleResponseDto>()
            .ForMember(dest => dest.Permissions,
                       opt => opt.MapFrom(src => src.Permissions.Select(p => p.Name).ToList()));

        CreateMap<Role, RoleWrapperDto>()
            .ForMember(dest => dest.Role,
                       opt => opt.MapFrom(src => src));

        CreateMap<RoleRequestDto, Role>();
    }
}

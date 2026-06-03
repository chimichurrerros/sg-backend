using BackEnd.DTOs.Requests.Permission;
using BackEnd.DTOs.Responses.Permission;
using BackEnd.Models;

namespace BackEnd.DTOs.Mappings;

public class PermissionMapper : AutoMapper.Profile
{
    public PermissionMapper()
    {
        CreateMap<Permission, PermissionResponseDto>()
            .ForMember(dest => dest.RoleName,
                       opt => opt.MapFrom(src => src.Role!.Name));

        CreateMap<Permission, PermissionWrapperDto>()
            .ForMember(dest => dest.Permission,
                       opt => opt.MapFrom(src => src));

        CreateMap<PermissionRequestDto, Permission>();
    }
}

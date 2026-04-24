using BackEnd.DTOs.Requests.User;
using BackEnd.DTOs.Responses.User;
using BackEnd.Models;

namespace BackEnd.DTOs.Mappings;

public class UserMapper : AutoMapper.Profile
{
    public UserMapper()
    {
        // Basic case: Property with the same name and type maps automatically
        // Advanced case: Map Role.Name to DTO RoleName
        CreateMap<User, UserResponseDto>()
            .ForMember(dest => dest.RoleName,
                       opt => opt.MapFrom(src => src.Role!.Name));

        CreateMap<User, UserWrapperDto>()
            .ForMember(dest => dest.User,
                       opt => opt.MapFrom(src => src));

        // Recomendable poner esta condicion
        CreateMap<UpdateUserRequestDto, User>()
            .ForAllMembers(opts => opts.Condition((src, dest, srcMember) => srcMember != null));

        // This mapping is not needed because we are projecting 
        // the users to the UserResponseDto and then wrapping them 
        // in the ListUsersWrapperDto

        // CreateMap<List<User>, ListUsersWrapperDto>()
        //     .ForMember(dest => dest.Users,
        //                opt => opt.MapFrom(src => src));
    }
}
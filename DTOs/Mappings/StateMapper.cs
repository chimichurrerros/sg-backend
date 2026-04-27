using BackEnd.DTOs.Requests.State;
using BackEnd.DTOs.Responses.State;
using BackEnd.Models;

namespace BackEnd.DTOs.Mappings;

public class StateMapper : AutoMapper.Profile
{
    public StateMapper()
    {
        CreateMap<State, StateResponseDto>();

        CreateMap<State, StateWrapperDto>()
            .ForMember(dest => dest.State,
                       opt => opt.MapFrom(src => src));

        CreateMap<StateRequestDto, State>();
    }
}

using BackEnd.DTOs.Requests.Position;
using BackEnd.DTOs.Responses.Position;
using BackEnd.Models;
using AutoMapper;

namespace BackEnd.DTOs.Mappings;

public class PositionMapper : Profile
{
    public PositionMapper()
    {
        CreateMap<PositionRequestDto, Position>();

        CreateMap<Position, PositionResponseDto>();

        CreateMap<Position, PositionWrapperDto>()
            .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src));
    }
}
using BackEnd.DTOs.Requests.Schedule;
using BackEnd.DTOs.Responses.Schedule;
using BackEnd.Models;
using AutoMapper;

namespace BackEnd.DTOs.Mappings;

public class ScheduleMapper : Profile
{
    public ScheduleMapper()
    {
        CreateMap<ScheduleRequestDto, Schedule>();

        CreateMap<Schedule, ScheduleResponseDto>()
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src =>
                src.ScheduleType == ScheduleTypeEnum.Morning ? "Turno Mañana" :
                src.ScheduleType == ScheduleTypeEnum.Afternoon ? "Turno Tarde" :
                src.ScheduleType == ScheduleTypeEnum.Night ? "Turno Noche" :
                src.ScheduleType == ScheduleTypeEnum.FullTime ? "Jornada Completa" :
                src.ScheduleType == ScheduleTypeEnum.PartTime ? "Medio Tiempo" :
                "Desconocido"));

        CreateMap<Schedule, ScheduleWrapperDto>()
            .ForMember(dest => dest.Schedule, opt => opt.MapFrom(src => src));
    }
}
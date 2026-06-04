using AutoMapper;
using BackEnd.DTOs.Responses.Employee;
using BackEnd.Models;

namespace BackEnd.DTOs.Mappings;

public class EmployeeMapper : Profile
{
    public EmployeeMapper()
    {
        CreateMap<Employee, EmployeeResponseDto>()
            .ForMember(dest => dest.AreaName, opt => opt.MapFrom(src => src.Area.Name))
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch.Name))
            .ForMember(dest => dest.BaseSalary, opt => opt.MapFrom(src =>
                src.PositionByScheduleByEmployees
                    .OrderByDescending(p => p.StartDate)
                    .Select(p => (decimal?)p.BasicSalary)
                    .FirstOrDefault()))
            .ForMember(dest => dest.PositionStartDate, opt => opt.MapFrom(src =>
                src.PositionByScheduleByEmployees
                    .OrderByDescending(p => p.StartDate)
                    .Select(p => (DateOnly?)p.StartDate)
                    .FirstOrDefault()))
            .ForMember(dest => dest.PositionId, opt => opt.MapFrom(src =>
                src.PositionByScheduleByEmployees
                    .OrderByDescending(p => p.StartDate)
                    .Select(p => (int?)p.PositionId)
                    .FirstOrDefault()))
            .ForMember(dest => dest.PositionName, opt => opt.MapFrom(src =>
                src.PositionByScheduleByEmployees
                    .OrderByDescending(p => p.StartDate)
                    .Select(p => p.Position.Name)
                    .FirstOrDefault()))
            .ForMember(dest => dest.ScheduleId, opt => opt.MapFrom(src =>
                src.PositionByScheduleByEmployees
                    .OrderByDescending(p => p.StartDate)
                    .Select(p => (int?)p.ScheduleId)
                    .FirstOrDefault()))
            .ForMember(dest => dest.ScheduleName, opt => opt.MapFrom(src =>
                src.PositionByScheduleByEmployees
                    .OrderByDescending(p => p.StartDate)
                    .Select(p => p.Schedule.ScheduleType == ScheduleTypeEnum.Morning ? "Turno Mañana" :
                        p.Schedule.ScheduleType == ScheduleTypeEnum.Afternoon ? "Turno Tarde" :
                        p.Schedule.ScheduleType == ScheduleTypeEnum.Night ? "Turno Noche" :
                        p.Schedule.ScheduleType == ScheduleTypeEnum.FullTime ? "Jornada Completa" :
                        p.Schedule.ScheduleType == ScheduleTypeEnum.PartTime ? "Medio Tiempo" :
                        "Desconocido")
                    .FirstOrDefault()));

        CreateMap<Employee, EmployeeWrapperDto>()
            .ForMember(dest => dest.Employee, opt => opt.MapFrom(src => src));
    }
}

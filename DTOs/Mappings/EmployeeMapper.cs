using AutoMapper;
using BackEnd.DTOs.Responses.Employee;
using BackEnd.Models;

namespace BackEnd.DTOs.Mappings;

public class EmployeeMapper : Profile
{
    public EmployeeMapper()
    {
        CreateMap<Employee, EmployeeResponseDto>()
            .ForMember(dest => dest.BaseSalary, opt => opt.MapFrom(src =>
                src.PositionByScheduleByEmployees
                    .OrderByDescending(p => p.StartDate)
                    .Select(p => (decimal?)p.BasicSalary)
                    .FirstOrDefault()))
            .ForMember(dest => dest.PositionStartDate, opt => opt.MapFrom(src =>
                src.PositionByScheduleByEmployees
                    .OrderByDescending(p => p.StartDate)
                    .Select(p => (DateOnly?)p.StartDate)
                    .FirstOrDefault()));

        CreateMap<Employee, EmployeeWrapperDto>()
            .ForMember(dest => dest.Employee, opt => opt.MapFrom(src => src));
    }
}

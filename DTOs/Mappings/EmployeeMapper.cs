using AutoMapper;
using BackEnd.DTOs.Responses.Employee;
using BackEnd.Models;

namespace BackEnd.DTOs.Mappings;

public class EmployeeMapper : Profile
{
    public EmployeeMapper()
    {
        CreateMap<Employee, EmployeeResponseDto>();

        CreateMap<Employee, EmployeeWrapperDto>()
            .ForMember(dest => dest.Employee, opt => opt.MapFrom(src => src));
    }
}

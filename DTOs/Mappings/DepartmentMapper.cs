using BackEnd.DTOs.Requests.Department;
using BackEnd.DTOs.Responses.Department;
using BackEnd.Models;
using AutoMapper;

namespace BackEnd.DTOs.Mappings;

public class DepartmentMapper : Profile
{
    public DepartmentMapper()
    {
        CreateMap<DepartmentRequestDto, Department>();

        CreateMap<Department, DepartmentResponseDto>();

        CreateMap<Department, DepartmentWrapperDto>()
            .ForMember(dest => dest.Department, opt => opt.MapFrom(src => src));
    }
}
using BackEnd.DTOs.Responses.Service;
using BackEnd.DTOs.Requests.Service;
using BackEnd.Models;

namespace BackEnd.DTOs.Mappings;

public class ServiceMapper : AutoMapper.Profile
{
    public ServiceMapper()
    {
        CreateMap<Product, ServiceResponseDto>();

        CreateMap<Product, ServiceWrapperDto>()
            .ForMember(dest => dest.Service,
                       opt => opt.MapFrom(src => src));

        CreateMap<ServiceRequestDto, Product>();

    }
}

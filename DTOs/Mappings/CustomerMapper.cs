using BackEnd.DTOs.Responses.Customer;
using BackEnd.Models;

namespace BackEnd.DTOs.Mappings;

public class CustomerMapper : AutoMapper.Profile
{
    public CustomerMapper()
    {
        CreateMap<Customer, CustomerResponseDto>();

        CreateMap<Customer, CustomerWrapperDto>()
            .ForMember(dest => dest.Customer,
                       opt => opt.MapFrom(src => src));
    }
}

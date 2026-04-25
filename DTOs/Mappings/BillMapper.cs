using BackEnd.DTOs.Responses.Bill;
using BackEnd.Models;

namespace BackEnd.DTOs.Mappings;

public class BillMapper : AutoMapper.Profile
{
    public BillMapper()
    {
        CreateMap<Bill, BillResponseDto>();

        CreateMap<Bill, BillWrapperDto>()
            .ForMember(dest => dest.Bill,
                opt => opt.MapFrom(src => src));
    }
}
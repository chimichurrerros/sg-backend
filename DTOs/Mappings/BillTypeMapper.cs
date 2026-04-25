using BackEnd.DTOs.Responses.BillType;
using BackEnd.Models;
using AutoMapper;

namespace BackEnd.DTOs.Mappings;

public class BillTypeMapper : Profile
{
    public BillTypeMapper()
    {
        CreateMap<BillType, BillTypeResponseDto>();

        CreateMap<BillType, BillTypeWrapperDto>()
            .ForMember(dest => dest.BillType,
                opt => opt.MapFrom(src => src));
    }
}
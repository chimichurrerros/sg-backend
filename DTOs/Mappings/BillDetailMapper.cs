using BackEnd.DTOs.Responses.BillDetail;
using BackEnd.Models;
using AutoMapper;

namespace BackEnd.DTOs.Mappings;

public class BillDetailMapper : Profile
{
    public BillDetailMapper()
    {
        CreateMap<BillDetail, BillDetailResponseDto>();

        CreateMap<BillDetail, BillDetailWrapperDto>()
            .ForMember(dest => dest.BillDetail,
                opt => opt.MapFrom(src => src));
    }
}
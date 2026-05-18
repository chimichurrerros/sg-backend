using BackEnd.DTOs.Responses.PurchaseRequest;
using BackEnd.Models;
using AutoMapper;

namespace BackEnd.DTOs.Mappings;

public class PurchaseRequestMapper : Profile
{
    public PurchaseRequestMapper()
    {
        CreateMap<PurchaseRequest, PurchaseRequestResponseDto>()
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => $"{src.User.Name} {src.User.LastName}"))
            .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.PurchaseRequestDetails));

        CreateMap<PurchaseRequestDetail, PurchaseRequestDetailResponseDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name));

        CreateMap<PurchaseRequest, PurchaseRequestWrapperDto>()
            .ForMember(dest => dest.PurchaseRequest, opt => opt.MapFrom(src => src));
    }
}

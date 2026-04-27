using BackEnd.DTOs.Responses.SalesOrder;
using BackEnd.DTOs.Responses.Bill;
using BackEnd.Models;
using AutoMapper;

namespace BackEnd.DTOs.Mappings;

public class SalesOrderMapper : Profile
{
    public SalesOrderMapper()
    {
        CreateMap<SalesOrder, SalesOrderResponseDto>()
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Customer.Name))
            .ForMember(dest => dest.UserName, opt => opt.MapFrom(src => $"{src.User.Name} {src.User.LastName}"))
            .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.SalesOrderDetails))
            .ForMember(dest => dest.Bills, opt => opt.MapFrom(src => src.Bills));

        CreateMap<SalesOrderDetail, SalesOrderDetailResponseDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name));

        CreateMap<SalesOrder, SalesOrderWrapperDto>()
            .ForMember(dest => dest.SalesOrder, opt => opt.MapFrom(src => src));
    }
}

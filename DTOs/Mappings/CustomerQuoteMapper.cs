using BackEnd.DTOs.Responses.CustomerQuote;
using BackEnd.Models;

namespace BackEnd.DTOs.Mappings;

public class CustomerQuoteMapper : AutoMapper.Profile
{
    public CustomerQuoteMapper()
    {
        CreateMap<CustomerQuoteDetail, CustomerQuoteDetailResponseDto>()
            .ForMember(dest => dest.ProductName,
                opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null));

        CreateMap<CustomerQuote, CustomerQuoteResponseDto>()
            .ForMember(dest => dest.CustomerName,
                opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : null))
             .ForMember(dest => dest.CustomerRuc,
                opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Ruc : null))
            .ForMember(dest => dest.CustomerBirthDate,
                opt => opt.MapFrom(src => src.Customer != null ? src.Customer.BirthDate : null))
            .ForMember(dest => dest.CustomerEmail,
                opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Email : null))
            .ForMember(dest => dest.UserName,
                opt => opt.MapFrom(src => src.User != null ? $"{src.User.Name} {src.User.LastName}".Trim() : null))
            .ForMember(dest => dest.Details,
                opt => opt.MapFrom(src => src.CustomerQuoteDetails))
            .ForMember(dest => dest.AssociatedSalesOrderId,
                opt => opt.MapFrom(src => (src.SalesOrders != null && src.SalesOrders.Any()) ? (int?)src.SalesOrders.OrderByDescending(o => o.Id).First().Id : null));

        CreateMap<CustomerQuote, CustomerQuoteWrapperDto>()
            .ForMember(dest => dest.CustomerQuote,
                opt => opt.MapFrom(src => src));
    }
}

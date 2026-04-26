using BackEnd.DTOs.Requests.CustomerQuote;
using BackEnd.DTOs.Responses.CustomerQuote;
using BackEnd.Models;

namespace BackEnd.DTOs.Mappings;

public class CustomerQuoteMapper : AutoMapper.Profile
{
    public CustomerQuoteMapper()
    {
        CreateMap<CustomerQuoteDetailRequestDto, CustomerQuoteDetail>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerQuoteId, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerQuote, opt => opt.Ignore())
            .ForMember(dest => dest.Product, opt => opt.Ignore());

        CreateMap<CreateCustomerQuoteRequestDto, CustomerQuote>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Date, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Total, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerQuoteDetails, opt => opt.MapFrom(src => src.Details))
            .ForMember(dest => dest.Customer, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.SalesOrders, opt => opt.Ignore());

        CreateMap<UpdateCustomerQuoteRequestDto, CustomerQuote>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Date, opt => opt.Ignore())
            .ForMember(dest => dest.Total, opt => opt.Ignore())
            .ForMember(dest => dest.Status, opt => opt.Ignore())
            .ForMember(dest => dest.CustomerQuoteDetails, opt => opt.MapFrom(src => src.Details))
            .ForMember(dest => dest.Customer, opt => opt.Ignore())
            .ForMember(dest => dest.User, opt => opt.Ignore())
            .ForMember(dest => dest.SalesOrders, opt => opt.Ignore());

        CreateMap<CustomerQuoteDetail, CustomerQuoteDetailResponseDto>()
            .ForMember(dest => dest.ProductName,
                opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null));

        CreateMap<CustomerQuote, CustomerQuoteResponseDto>()
            .ForMember(dest => dest.CustomerName,
                opt => opt.MapFrom(src => src.Customer != null ? src.Customer.Name : null))
            .ForMember(dest => dest.UserName,
                opt => opt.MapFrom(src => src.User != null ? $"{src.User.Name} {src.User.LastName}".Trim() : null))
            .ForMember(dest => dest.Details,
                opt => opt.MapFrom(src => src.CustomerQuoteDetails));

        CreateMap<CustomerQuote, CustomerQuoteWrapperDto>()
            .ForMember(dest => dest.CustomerQuote,
                opt => opt.MapFrom(src => src));
    }
}

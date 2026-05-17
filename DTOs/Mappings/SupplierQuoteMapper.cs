using BackEnd.DTOs.Requests.SupplierQuote;
using BackEnd.DTOs.Responses.SupplierQuote;
using BackEnd.Models;

namespace BackEnd.DTOs.Mappings;

public class SupplierQuoteMapper : AutoMapper.Profile
{
    public SupplierQuoteMapper()
    {
        CreateMap<SupplierQuoteDetailRequestDto, SupplierQuoteDetail>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.SupplierQuoteId, opt => opt.Ignore())
            .ForMember(dest => dest.SupplierQuote, opt => opt.Ignore())
            .ForMember(dest => dest.Product, opt => opt.Ignore());

        CreateMap<CreateSupplierQuoteRequestDto, SupplierQuote>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Date, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Total, opt => opt.Ignore())
            .ForMember(dest => dest.SupplierQuoteDetails, opt => opt.MapFrom(src => src.Details))
            .ForMember(dest => dest.PurchaseRequest, opt => opt.Ignore())
            .ForMember(dest => dest.Supplier, opt => opt.Ignore())
            .ForMember(dest => dest.State, opt => opt.Ignore())
            .ForMember(dest => dest.PurchaseOrders, opt => opt.Ignore());

        CreateMap<UpdateSupplierQuoteRequestDto, SupplierQuote>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Date, opt => opt.Ignore())
            .ForMember(dest => dest.Total, opt => opt.Ignore())
            .ForMember(dest => dest.SupplierQuoteDetails, opt => opt.MapFrom(src => src.Details))
            .ForMember(dest => dest.PurchaseRequest, opt => opt.Ignore())
            .ForMember(dest => dest.Supplier, opt => opt.Ignore())
            .ForMember(dest => dest.State, opt => opt.Ignore())
            .ForMember(dest => dest.PurchaseOrders, opt => opt.Ignore());

        CreateMap<SupplierQuoteDetail, SupplierQuoteDetailResponseDto>()
            .ForMember(dest => dest.ProductName,
                opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null));

        CreateMap<SupplierQuote, SupplierQuoteResponseDto>()
            .ForMember(dest => dest.SupplierName,
                opt => opt.MapFrom(src => src.Supplier != null ? (string.IsNullOrWhiteSpace(src.Supplier.FantasyName) ? src.Supplier.BusinessName : src.Supplier.FantasyName) : null))
            .ForMember(dest => dest.Details,
                opt => opt.MapFrom(src => src.SupplierQuoteDetails));

        CreateMap<SupplierQuote, SupplierQuoteWrapperDto>()
            .ForMember(dest => dest.SupplierQuote, opt => opt.MapFrom(src => src));
    }
}

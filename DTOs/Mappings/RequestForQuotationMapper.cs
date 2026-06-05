using BackEnd.DTOs.Responses.RequestForQuotation;
using BackEnd.Models;

namespace BackEnd.DTOs.Mappings;

public class RequestForQuotationMapper : AutoMapper.Profile
{
    public RequestForQuotationMapper()
    {
        CreateMap<RequestForQuotationDetail, RequestForQuotationProductDto>()
            .ForMember(dest => dest.ProductId, opt => opt.MapFrom(src => src.ProductId))
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
            .ForMember(dest => dest.QuantityRequested, opt => opt.MapFrom(src => src.QuantityRequested))
            .ForMember(dest => dest.CategoryId, opt => opt.MapFrom(src => src.Product != null ? src.Product.ProductCategoryId : null))
            .ForMember(dest => dest.CategoryName, opt => opt.MapFrom(src => src.Product != null && src.Product.ProductCategory != null ? src.Product.ProductCategory.Name : null))
            .ForMember(dest => dest.ProductCost, opt => opt.MapFrom(src => src.Product != null ? src.Product.Cost : 0));

        CreateMap<RequestForQuotation, RequestForQuotationResponseDto>()
            .ForMember(dest => dest.SupplierName,
                opt => opt.MapFrom(src => src.Supplier != null
                    ? (string.IsNullOrWhiteSpace(src.Supplier.FantasyName) ? src.Supplier.BusinessName : src.Supplier.FantasyName)
                    : null))
            .ForMember(dest => dest.Products, opt => opt.MapFrom(src => src.RequestForQuotationDetails))
            .ForMember(dest => dest.PurchaseRequestDate, opt => opt.MapFrom(src => src.PurchaseRequest.Date))
            .ForMember(dest => dest.PurchaseRequestState, opt => opt.MapFrom(src => src.PurchaseRequest.PurchaseRequestState))
            .ForMember(dest => dest.PurchaseRequestObservation, opt => opt.MapFrom(src => src.PurchaseRequest.Observation));

        CreateMap<RequestForQuotation, RequestForQuotationWrapperDto>()
            .ForMember(dest => dest.RequestForQuotation, opt => opt.MapFrom(src => src));
    }
}

using BackEnd.DTOs.Requests.PurchaseOrder;
using BackEnd.DTOs.Responses.PurchaseOrder;
using BackEnd.Models;

namespace BackEnd.DTOs.Mappings;

public class PurchaseOrderMapper : AutoMapper.Profile
{
    public PurchaseOrderMapper()
    {
        CreateMap<PurchaseOrderDetailRequestDto, PurchaseOrderDetail>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PurchaseOrderId, opt => opt.Ignore())
            .ForMember(dest => dest.PurchaseOrder, opt => opt.Ignore())
            .ForMember(dest => dest.Product, opt => opt.Ignore())
            .ForMember(dest => dest.SupplierQuoteDetail, opt => opt.Ignore())
            .ForMember(dest => dest.QuantityReceived, opt => opt.Ignore())
            .ForMember(dest => dest.Price, opt => opt.Ignore())
            .ForMember(dest => dest.TaxRate, opt => opt.Ignore());

        CreateMap<CreatePurchaseOrderRequestDto, PurchaseOrder>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Number, opt => opt.Ignore())
            .ForMember(dest => dest.Date, opt => opt.MapFrom(_ => DateTime.UtcNow))
            .ForMember(dest => dest.Total, opt => opt.Ignore())
            .ForMember(dest => dest.PurchaseRequest, opt => opt.Ignore())
            .ForMember(dest => dest.Supplier, opt => opt.Ignore())
            .ForMember(dest => dest.State, opt => opt.Ignore())
            .ForMember(dest => dest.SupplierQuote, opt => opt.Ignore())
            .ForMember(dest => dest.Bills, opt => opt.Ignore())
            .ForMember(dest => dest.PurchaseOrderDetails, opt => opt.MapFrom(src => src.Details));

        CreateMap<UpdatePurchaseOrderRequestDto, PurchaseOrder>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Number, opt => opt.Ignore())
            .ForMember(dest => dest.Date, opt => opt.Ignore())
            .ForMember(dest => dest.Total, opt => opt.Ignore())
            .ForMember(dest => dest.PurchaseRequest, opt => opt.Ignore())
            .ForMember(dest => dest.Supplier, opt => opt.Ignore())
            .ForMember(dest => dest.State, opt => opt.Ignore())
            .ForMember(dest => dest.SupplierQuote, opt => opt.Ignore())
            .ForMember(dest => dest.Bills, opt => opt.Ignore())
            .ForMember(dest => dest.PurchaseOrderDetails, opt => opt.MapFrom(src => src.Details));

        CreateMap<PurchaseOrderDetail, PurchaseOrderDetailResponseDto>()
            .ForMember(dest => dest.ProductName,
                opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null))
            .ForMember(dest => dest.SupplierQuoteDetailId,
                opt => opt.MapFrom(src => src.SupplierQuoteDetailId))
            .ForMember(dest => dest.SupplierQuoteId,
                opt => opt.MapFrom(src => src.SupplierQuoteDetail != null ? (int?)src.SupplierQuoteDetail.SupplierQuoteId : null))
            .ForMember(dest => dest.SupplierId,
                opt => opt.MapFrom(src => src.SupplierQuoteDetail != null && src.SupplierQuoteDetail.SupplierQuote != null ? (int?)src.SupplierQuoteDetail.SupplierQuote.SupplierId : null))
            .ForMember(dest => dest.SupplierName,
                opt => opt.MapFrom(src => src.SupplierQuoteDetail != null && src.SupplierQuoteDetail.SupplierQuote != null && src.SupplierQuoteDetail.SupplierQuote.Supplier != null
                    ? (string.IsNullOrWhiteSpace(src.SupplierQuoteDetail.SupplierQuote.Supplier.FantasyName)
                        ? src.SupplierQuoteDetail.SupplierQuote.Supplier.BusinessName
                        : src.SupplierQuoteDetail.SupplierQuote.Supplier.FantasyName)
                    : null));

        CreateMap<PurchaseOrder, PurchaseOrderResponseDto>()
            .ForMember(dest => dest.SupplierName,
                opt => opt.MapFrom(src => src.Supplier != null ? (string.IsNullOrWhiteSpace(src.Supplier.FantasyName) ? src.Supplier.BusinessName : src.Supplier.FantasyName) : null))
            .ForMember(dest => dest.Details,
                opt => opt.MapFrom(src => src.PurchaseOrderDetails));

        CreateMap<PurchaseOrder, PurchaseOrderWrapperDto>()
            .ForMember(dest => dest.PurchaseOrder, opt => opt.MapFrom(src => src));

        CreateMap<PurchaseOrder, PurchaseOrderDraftWrapperDto>()
            .ForMember(dest => dest.PurchaseOrder, opt => opt.MapFrom(src => src));
    }
}

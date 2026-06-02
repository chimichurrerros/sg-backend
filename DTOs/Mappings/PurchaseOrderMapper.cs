using BackEnd.DTOs.Requests.PurchaseOrder;
using BackEnd.DTOs.Responses.PurchaseOrder;
using BackEnd.Models;

namespace BackEnd.DTOs.Mappings;

public class PurchaseOrderMapper : AutoMapper.Profile
{
    public PurchaseOrderMapper()
    {
        CreateMap<PurchaseOrder, PurchaseOrderResponseDto>()
            .ForMember(dest => dest.PurchaseOrdersForSupplier, opt => opt.MapFrom(src => src.PurchaseOrdersForSupplier));

        CreateMap<PurchaseOrder, PurchaseOrderWrapperDto>()
            .ForMember(dest => dest.PurchaseOrder, opt => opt.MapFrom(src => src));

        CreateMap<PurchaseOrder, PurchaseOrderDraftWrapperDto>()
            .ForMember(dest => dest.PurchaseOrder, opt => opt.MapFrom(src => src));

        CreateMap<PurchaseOrderForSupplier, PurchaseOrderForSupplierResponseDto>()
            .ForMember(dest => dest.Supplier,
                opt => opt.MapFrom(src => src.Supplier))
            .ForMember(dest => dest.SupplierName,
                opt => opt.MapFrom(src => src.Supplier != null ? (string.IsNullOrWhiteSpace(src.Supplier.FantasyName) ? src.Supplier.BusinessName : src.Supplier.FantasyName) : null))
            .ForMember(dest => dest.Details,
                opt => opt.MapFrom(src => src.PurchaseOrderDetails));

        CreateMap<PurchaseOrderForSupplier, PurchaseOrderForSupplierWrapperDto>()
            .ForMember(dest => dest.PurchaseOrderForSupplier, opt => opt.MapFrom(src => src));

        CreateMap<PurchaseOrderDetail, PurchaseOrderForSupplierDetailResponseDto>()
            .ForMember(dest => dest.ProductName,
                opt => opt.MapFrom(src => src.Product != null ? src.Product.Name : null));
    }
}

using BackEnd.DTOs.Requests.Product;
using BackEnd.DTOs.Responses.Product;
using BackEnd.Models;

namespace BackEnd.DTOs.Mappings;

public class ProductMapper : AutoMapper.Profile
{
    public ProductMapper()
    {
        CreateMap<Product, ProductResponseDto>()
            .ForMember(dest => dest.ProductCategoryName,
                       opt => opt.MapFrom(src => src.ProductCategory!.Name))
            .ForMember(dest => dest.ProductBrandName,
                       opt => opt.MapFrom(src => src.ProductBrand!.Name));

        CreateMap<Product, ProductWrapperDto>()
            .ForMember(dest => dest.Product,
                       opt => opt.MapFrom(src => src));

        CreateMap<Stock, ProductStockResponseDto>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Product.Id))
            .ForMember(dest => dest.ProductCategoryId, opt => opt.MapFrom(src => src.Product.ProductCategoryId))
            .ForMember(dest => dest.ProductCategoryName, opt => opt.MapFrom(src => src.Product.ProductCategory!.Name))
            .ForMember(dest => dest.ProductBrandId, opt => opt.MapFrom(src => src.Product.ProductBrandId))
            .ForMember(dest => dest.ProductBrandName, opt => opt.MapFrom(src => src.Product.ProductBrand!.Name))
            .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Product.Description))
            .ForMember(dest => dest.Barcode, opt => opt.MapFrom(src => src.Product.Barcode))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Product.Price))
            .ForMember(dest => dest.Cost, opt => opt.MapFrom(src => src.Product.Cost))
            .ForMember(dest => dest.TaxRate, opt => opt.MapFrom(src => src.Product.TaxRate))
            .ForMember(dest => dest.MinimumStock, opt => opt.MapFrom(src => src.Product.MinimumStock))
            .ForMember(dest => dest.Quantity, opt => opt.MapFrom(src => src.Quantity));

        CreateMap<ProductRequestDto, Product>();
    }
}

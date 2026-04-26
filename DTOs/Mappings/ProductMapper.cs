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

        CreateMap<ProductRequestDto, Product>();
    }
}

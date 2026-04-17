using BackEnd.DTOs.Responses.ProductCategory;
using BackEnd.Models;

namespace BackEnd.DTOs.Mappings;

public class ProductCategoryMapper : AutoMapper.Profile
{
    public ProductCategoryMapper()
    {
        CreateMap<ProductCategory, ProductCategoryResponseDto>();

        CreateMap<ProductCategory, ProductCategoryWrapperDto>()
            .ForMember(dest => dest.ProductCategory,
                       opt => opt.MapFrom(src => src));
    }
}

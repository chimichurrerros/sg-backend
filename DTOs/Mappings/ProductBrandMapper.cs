using BackEnd.DTOs.Requests.ProductBrand;
using BackEnd.DTOs.Responses.ProductBrand;
using BackEnd.Models;

namespace BackEnd.DTOs.Mappings;

public class ProductBrandMapper : AutoMapper.Profile
{
    public ProductBrandMapper()
    {
        CreateMap<ProductBrand, ProductBrandResponseDto>();

        CreateMap<ProductBrand, ProductBrandWrapperDto>()
            .ForMember(dest => dest.ProductBrand,
                       opt => opt.MapFrom(src => src));

        CreateMap<ProductBrandRequestDto, ProductBrand>();
    }
}

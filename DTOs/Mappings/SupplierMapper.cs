using BackEnd.DTOs.Responses.Supplier;
using BackEnd.Models;

namespace BackEnd.DTOs.Mappings;

public class SupplierMapper : AutoMapper.Profile
{
    public SupplierMapper()
    {
        CreateMap<Supplier, SupplierResponseDto>();

        CreateMap<Supplier, SupplierWrapperDto>()
            .ForMember(dest => dest.Supplier,
                       opt => opt.MapFrom(src => src));
    }
}

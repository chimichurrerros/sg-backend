using BackEnd.DTOs.Requests.Stock;
using BackEnd.DTOs.Responses.Stock;
using BackEnd.Models;

namespace BackEnd.DTOs.Mappings;

public class StockMapper : AutoMapper.Profile
{
    public StockMapper()
    {
        CreateMap<Stock, StockResponseDto>()
            .ForMember(dest => dest.ProductName,
                opt => opt.MapFrom(src => src.Product.Name))
            .ForMember(dest => dest.BranchName,
                opt => opt.MapFrom(src => src.Branch.Name));

        CreateMap<StockRequestDto, Stock>();
    }
}
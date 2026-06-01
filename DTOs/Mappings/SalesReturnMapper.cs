using BackEnd.DTOs.Responses.CreditNote;
using BackEnd.DTOs.Responses.SalesReturn;
using BackEnd.Models;
using AutoMapper;

namespace BackEnd.DTOs.Mappings;

public class SalesReturnMapper : Profile
{
    public SalesReturnMapper()
    {
        CreateMap<SalesReturn, SalesReturnResponseDto>()
            .ForMember(dest => dest.BranchName, opt => opt.MapFrom(src => src.Branch.Name))
            .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.CreditNote!.CreditNoteDetails));

        CreateMap<CreditNoteDetail, CreditNoteDetailResponseDto>()
            .ForMember(dest => dest.ProductName, opt => opt.MapFrom(src => src.Product.Name));

        CreateMap<SalesReturnResponseDto, SalesReturnWrapperDto>()
            .ForMember(dest => dest.SalesReturn, opt => opt.MapFrom(src => src));
    }
}

using AutoMapper;
using BackEnd.DTOs.Responses.CreditNote;
using BackEnd.Models;

namespace BackEnd.DTOs.Mappings;

public class CreditNoteMapper : Profile
{
    public CreditNoteMapper()
    {
        CreateMap<CreditNote, CreditNoteResponseDto>()
            .ForMember(dest => dest.BillNumber, opt => opt.MapFrom(src => src.Bill.Number))
            .ForMember(dest => dest.CustomerId, opt => opt.MapFrom(src => src.Bill.CustomerId ?? 0))
            .ForMember(dest => dest.CustomerName, opt => opt.MapFrom(src => src.Bill.Customer != null ? src.Bill.Customer.Name : string.Empty))
            .ForMember(dest => dest.CustomerRuc, opt => opt.MapFrom(src => src.Bill.Customer != null ? src.Bill.Customer.Ruc : string.Empty))
            .ForMember(dest => dest.Details, opt => opt.MapFrom(src => src.CreditNoteDetails));
    }
}

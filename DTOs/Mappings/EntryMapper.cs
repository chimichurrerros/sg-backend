using BackEnd.DTOs.Requests.Entry;
using BackEnd.DTOs.Responses.Entry;
using BackEnd.Models;

namespace BackEnd.DTOs.Mappings;

public class EntryMapper : AutoMapper.Profile
{
    public EntryMapper()
    {
        CreateMap<Entry, EntryResponseDto>();
        CreateMap<EntryDetail, EntryDetailResponseDto>();

        CreateMap<Entry, EntryWrapperDto>()
            .ForMember(dest => dest.Entry,
                       opt => opt.MapFrom(src => src));

        CreateMap<CreateEntryRequestDto, Entry>();
        CreateMap<CreateEntryDetailDto, EntryDetail>();

        CreateMap<UpdateEntryRequestDto, Entry>();
        CreateMap<UpdateEntryDetailDto, EntryDetail>();
    }
}

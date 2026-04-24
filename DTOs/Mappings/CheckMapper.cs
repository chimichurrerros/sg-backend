using AutoMapper;
using BackEnd.Models;
using BackEnd.DTOs.Requests.Checks; // Ajusta según tu namespace
using BackEnd.DTOs.Responses.Checks; // Ajusta según tu namespace

namespace BackEnd.Profiles;

public class CheckProfile : Profile
{
    public CheckProfile()
    {
        // De Request a Entidad (Para crear)
        CreateMap<CreateCheckRequestDto, Check>();

        // De Entidad a Response (Para devolver al frontend)
        CreateMap<Check, CheckResponseDto>();
    }
}
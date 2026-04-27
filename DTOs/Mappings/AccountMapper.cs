using AutoMapper;
using BackEnd.Models;
using BackEnd.DTOs.Requests.Accounts;
namespace BackEnd.Profiles;

public class AccountProfile : Profile
{
    public AccountProfile()
    {

        // De Request a Entidad (Para actualizar)
        CreateMap<UpdateAccountRequestDto, Account>();
        // De Entidad a Response (Para devolver al frontend)
        CreateMap<Account, AccountResponseDto>();
    }
}
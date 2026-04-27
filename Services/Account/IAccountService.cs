using BackEnd.DTOs.Requests.Accounts;
using BackEnd.DTOs.Responses.Accounts;
using BackEnd.Utils;

namespace BackEnd.Services.Interfaces;

public interface IAccountService
{
    Task<Result<IEnumerable<AccountResponseDto>>> GetAllAsync();
    Task<Result<AccountResponseDto>> GetByIdAsync(int id);
    Task<Result<AccountResponseDto>> CreateAsync(CreateAccountRequestDto request);
    Task<Result<AccountResponseDto>> UpdateAsync(int id, UpdateAccountRequestDto request);
    Task<Result<bool>> DeleteAsync(int id);
}
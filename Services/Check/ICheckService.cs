using BackEnd.DTOs.Requests.Checks;
using BackEnd.DTOs.Responses.Checks;

using BackEnd.Utils;

namespace BackEnd.Services.Interfaces;

public interface ICheckService
{
    Task<Result<IEnumerable<CheckResponseDto>>> GetAllAsync();
    Task<Result<CheckResponseDto>> GetByIdAsync(int id);
    Task<Result<CheckResponseDto>> CreateAsync(CreateCheckRequestDto request);
    Task<Result<CheckResponseDto>> UpdateStatusAsync(int id, UpdateCheckStatusRequestDto request);
}
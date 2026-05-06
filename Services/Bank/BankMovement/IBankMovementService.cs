using BackEnd.DTOs.Requests.Bank.BankMovement; 
using BackEnd.DTOs.Responses.Bank.BankMovement;
using BackEnd.Utils;

namespace BackEnd.Services.Interfaces;

public interface IBankMovementService
{
    Task<Result<IEnumerable<BankMovementResponseDto>>> GetAllAsync();
    Task<Result<BankMovementResponseDto>> GetByIdAsync(int id);
    Task<Result<BankMovementResponseDto>> CreateAsync(BankMovementRequestDto request);
}
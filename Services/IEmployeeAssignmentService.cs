using BackEnd.DTOs.Requests.Employee;
using BackEnd.Utils;

namespace BackEnd.Services;

public interface IEmployeeAssignmentService
{
    Task<Result<PositionByScheduleByEmployeeDto>> AssignPositionAndScheduleAsync(PositionByScheduleByEmployeeDto dto);
}
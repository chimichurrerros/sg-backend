namespace BackEnd.DTOs.Responses.Employee;

public class EmployeePositionHistoryResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public int PositionId { get; set; }
    public string PositionName { get; set; } = null!;
    public int ScheduleId { get; set; }
    public BackEnd.Models.ScheduleTypeEnum ScheduleType { get; set; }
    public string ScheduleName { get; set; } = null!;
    public decimal BasicSalary { get; set; }
    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
}

public class EmployeePositionHistoryWrapperDto
{
    public EmployeePositionHistoryResponseDto History { get; set; } = null!;
}

public class ListEmployeePositionHistoriesWrapperDto
{
    public List<EmployeePositionHistoryResponseDto> Histories { get; set; } = [];
}

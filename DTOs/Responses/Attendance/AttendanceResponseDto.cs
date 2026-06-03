namespace BackEnd.DTOs.Responses.Attendance;

public class AttendanceResponseDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeFullName { get; set; } = null!;
    public string Date { get; set; } = null!;
    public int Status { get; set; }
    public string StatusName { get; set; } = null!;
}

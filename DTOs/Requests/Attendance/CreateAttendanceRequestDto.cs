namespace BackEnd.DTOs.Requests.Attendance;

public class CreateAttendanceRequestDto
{
    public int EmployeeId { get; set; }
    public DateOnly Date { get; set; }
    public int Status { get; set; }
}

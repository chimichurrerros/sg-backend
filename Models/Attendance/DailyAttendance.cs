namespace BackEnd.Models;

public partial class DailyAttendance
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DateOnly Date { get; set; }
    public AttendanceStatus Status { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}

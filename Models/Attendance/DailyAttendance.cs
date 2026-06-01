namespace BackEnd.Models;

public enum AttendanceStatus
{
    Present = 1,
    Absent = 2,
    Late = 3,
    ExcusedAbsence = 4,
    ExcusedLate = 5
}

public partial class DailyAttendance
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public DateOnly Date { get; set; }
    public AttendanceStatus Status { get; set; }

    public virtual Employee Employee { get; set; } = null!;
}

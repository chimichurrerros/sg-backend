namespace BackEnd.DTOs.Requests.Schedule;

public class ScheduleRequestDto
{
    public TimeOnly ArrivalTime { get; set; }
    public TimeOnly DepartureTime { get; set; }
    public decimal NumberOfHours { get; set; }
    public BackEnd.Models.ScheduleTypeEnum ScheduleType { get; set; }
}
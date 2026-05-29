using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.Schedule;

public class ScheduleResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public TimeOnly ArrivalTime { get; set; }
    public TimeOnly DepartureTime { get; set; }
    public decimal NumberOfHours { get; set; }
    public BackEnd.Models.ScheduleTypeEnum ScheduleType { get; set; }
}

public class ScheduleWrapperDto
{
    public ScheduleResponseDto Schedule { get; set; } = null!;
}

public class ListSchedulesWrapperDto
{
    public List<ScheduleResponseDto> Schedules { get; set; } = [];
    public Pagination Pagination { get; set; } = null!;
}
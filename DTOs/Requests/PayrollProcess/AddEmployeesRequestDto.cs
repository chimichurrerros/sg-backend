using System.Text.Json.Serialization;

namespace BackEnd.DTOs.Requests.PayrollProcess;

public class AddEmployeesRequestDto
{
    [JsonPropertyName("employeeIds")]
    public int[] EmployeeIds { get; set; } = [];
}

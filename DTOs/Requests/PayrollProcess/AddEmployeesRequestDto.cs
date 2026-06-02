<<<<<<< HEAD
=======
using System.Text.Json.Serialization;

>>>>>>> f2106ea2e1fd48c0df24fb0c460216e542820eaf
namespace BackEnd.DTOs.Requests.PayrollProcess;

public class AddEmployeesRequestDto
{
<<<<<<< HEAD
    public List<int> EmployeeIds { get; set; } = [];
=======
    [JsonPropertyName("employeeIds")]
    public int[] EmployeeIds { get; set; } = [];
>>>>>>> f2106ea2e1fd48c0df24fb0c460216e542820eaf
}

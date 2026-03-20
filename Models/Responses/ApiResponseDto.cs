namespace BackEnd.Models.Responses;

public class ApiResponseDto
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public object? Data { get; set; }
    public object? Errors { get; set; }
}
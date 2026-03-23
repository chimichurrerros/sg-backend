namespace BackEnd.Models.Responses;

public class ApiResponseDto<Type>
{
    public bool Success { get; set; }
    public Type? Data { get; set; }
    public string? Message { get; set; }
    public object? Errors { get; set; }
}
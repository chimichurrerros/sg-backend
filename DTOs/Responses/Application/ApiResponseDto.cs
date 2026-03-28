namespace BackEnd.DTOs.Responses.Application;

public class ApiResponseDto
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
}

public class ApiResponseDto<T> : ApiResponseDto
{
    public T? Data { get; set; }
}
namespace BackEnd.Models;

public class Result
{
    public bool IsSuccess { get; }
    public string? ErrorMessage { get; }
    public Dictionary<string, string[]>? Errors { get; }

    protected Result(bool isSuccess, string? errorMessage, Dictionary<string, string[]>? errors)
    {
        IsSuccess = isSuccess;
        ErrorMessage = errorMessage;
        Errors = errors;
    }

    public static Result Success() => new Result(true, null, null);
    public static Result Failure(string errorMessage) => new Result(false, errorMessage, null);
    public static Result Failure(string errorMessage, Dictionary<string, string[]> errors) => new Result(false, errorMessage, errors);
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, T? value, string? errorMessage, Dictionary<string, string[]>? errors) 
        : base(isSuccess, errorMessage, errors)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new Result<T>(true, value, null, null);
    public static new Result<T> Failure(string errorMessage) => new Result<T>(false, default, errorMessage, null);
    public static new Result<T> Failure(string errorMessage, Dictionary<string, string[]> errors) => new Result<T>(false, default, errorMessage, errors);
}

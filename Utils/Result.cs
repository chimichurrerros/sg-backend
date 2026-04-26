namespace BackEnd.Utils;

public class Result
{
    public bool IsSuccess { get; }
    public ErrorType ErrorType { get; }
    public string? ErrorMessage { get; }
    public Dictionary<string, string[]>? Errors { get; }

    protected Result(bool isSuccess, ErrorType errorType, string? errorMessage, Dictionary<string, string[]>? errors)
    {
        IsSuccess = isSuccess;
        ErrorType = errorType;
        ErrorMessage = errorMessage;
        Errors = errors;
    }

    public static Result Success() => new Result(true, ErrorType.None, null, null);
    public static Result Failure(string errorMessage, ErrorType errorType) => new Result(false, errorType, errorMessage, null);
    public static Result Failure(string errorMessage, Dictionary<string, string[]> errors, ErrorType errorType) => new Result(false, errorType, errorMessage, errors);
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, ErrorType errorType, T? value, string? errorMessage, Dictionary<string, string[]>? errors) 
        : base(isSuccess, errorType, errorMessage, errors)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new Result<T>(true, ErrorType.None, value, null, null);
    public static new Result<T> Failure(string errorMessage, ErrorType errorType) => new Result<T>(false, errorType, default, errorMessage, null);
    public static new Result<T> Failure(string errorMessage, Dictionary<string, string[]> errors, ErrorType errorType) => new Result<T>(false, errorType, default, errorMessage, errors);
}

public enum ErrorType
{
    None = 0,
    Failure = 1,
    Validation = 2,
    NotFound = 3,
    Conflict = 4,
    Unauthorized = 5,
    Forbidden = 6,
    Unexpected = 7
}
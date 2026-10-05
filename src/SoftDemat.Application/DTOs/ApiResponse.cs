namespace SoftDemat.Application.DTOs;

public sealed record ApiResponse<T>(bool Success, string Message, T? Data, DateTime Timestamp)
{
    public static ApiResponse<T> Ok(T data, string message = "")
        => new(true, message, data, DateTime.UtcNow);

    public static ApiResponse<T> Fail(string message)
        => new(false, message, default, DateTime.UtcNow);
}

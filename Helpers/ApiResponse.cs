namespace EmployeeEvaluationSystem.Helpers;

/// <summary>
/// Standard API envelope used by every controller in this project:
/// { "success": true, "message": "...", "data": { } } or
/// { "success": false, "message": "..." }
/// </summary>
public class ApiResponse<T>
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;
    public T? Data { get; set; }

    public static ApiResponse<T> SuccessResponse(T data, string message = "Request completed successfully.")
    {
        return new ApiResponse<T> { Success = true, Message = message, Data = data };
    }

    public static ApiResponse<T> FailResponse(string message)
    {
        return new ApiResponse<T> { Success = false, Message = message, Data = default };
    }
}

/// <summary>Non-generic convenience wrapper for endpoints that return no payload.</summary>
public class ApiResponse
{
    public bool Success { get; set; }
    public string Message { get; set; } = string.Empty;

    public static ApiResponse SuccessResponse(string message = "Request completed successfully.")
    {
        return new ApiResponse { Success = true, Message = message };
    }

    public static ApiResponse FailResponse(string message)
    {
        return new ApiResponse { Success = false, Message = message };
    }
}

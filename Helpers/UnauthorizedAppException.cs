using System.Net;

namespace EmployeeEvaluationSystem.Helpers;

/// <summary>Thrown for authentication failures (bad credentials, invalid/expired token). Maps to HTTP 401.</summary>
public class UnauthorizedAppException : AppException
{
    public UnauthorizedAppException(string message = "Invalid credentials.")
        : base(message, HttpStatusCode.Unauthorized)
    {
    }
}

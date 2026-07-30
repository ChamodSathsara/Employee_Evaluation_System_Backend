using System.Net;

namespace EmployeeEvaluationSystem.Helpers;

/// <summary>Thrown when an authenticated user is not permitted to perform the requested action. Maps to HTTP 403.</summary>
public class ForbiddenException : AppException
{
    public ForbiddenException(string message = "You are not authorized to perform this action.")
        : base(message, HttpStatusCode.Forbidden)
    {
    }
}

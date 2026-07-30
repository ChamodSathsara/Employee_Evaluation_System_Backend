using System.Net;

namespace EmployeeEvaluationSystem.Helpers;

/// <summary>Thrown when a request conflicts with the current state of a resource (e.g. duplicate email, editing a published KPI). Maps to HTTP 409.</summary>
public class ConflictException : AppException
{
    public ConflictException(string message) : base(message, HttpStatusCode.Conflict)
    {
    }
}

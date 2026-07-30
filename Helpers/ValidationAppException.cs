using System.Net;

namespace EmployeeEvaluationSystem.Helpers;

/// <summary>Thrown when business/input validation fails. Maps to HTTP 400.</summary>
public class ValidationAppException : AppException
{
    public IReadOnlyList<string> Errors { get; }

    public ValidationAppException(string message) : base(message, HttpStatusCode.BadRequest)
    {
        Errors = new List<string> { message };
    }

    public ValidationAppException(IEnumerable<string> errors)
        : base("One or more validation errors occurred.", HttpStatusCode.BadRequest)
    {
        Errors = errors.ToList();
    }
}

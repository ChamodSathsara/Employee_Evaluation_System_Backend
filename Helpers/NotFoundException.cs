using System.Net;

namespace EmployeeEvaluationSystem.Helpers;

/// <summary>Thrown when a requested entity does not exist. Maps to HTTP 404.</summary>
public class NotFoundException : AppException
{
    public NotFoundException(string entityName, object key)
        : base($"{entityName} with identifier '{key}' was not found.", HttpStatusCode.NotFound)
    {
    }

    public NotFoundException(string message) : base(message, HttpStatusCode.NotFound)
    {
    }
}

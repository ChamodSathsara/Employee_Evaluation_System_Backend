using System.Net;

namespace EmployeeEvaluationSystem.Helpers;

/// <summary>
/// Base type for all application-level (business/validation) exceptions.
/// Caught centrally by <see cref="EmployeeEvaluationSystem.Middleware.ExceptionHandlingMiddleware"/>
/// and translated into the standard failure response with the matching HTTP status code.
/// </summary>
public abstract class AppException : Exception
{
    public HttpStatusCode StatusCode { get; }

    protected AppException(string message, HttpStatusCode statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}

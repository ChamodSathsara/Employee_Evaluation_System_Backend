namespace EmployeeEvaluationSystem.Services;

public interface IEmailService
{
    Task SendKpiPublishedAsync(
        string recipientEmail,
        string recipientName,
        string periodName,
        CancellationToken cancellationToken = default);
}

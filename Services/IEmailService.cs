namespace EmployeeEvaluationSystem.Services;

public interface IEmailService
{
    Task SendKpiPublishedAsync(
        string recipientEmail,
        string recipientName,
        string periodName,
        string assignedByName,
        string designation,
        string department,
        int kpiCount,
        CancellationToken cancellationToken = default);
}

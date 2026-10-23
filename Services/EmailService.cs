using System.Net;
using System.Net.Mail;

namespace EmployeeEvaluationSystem.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendKpiPublishedAsync(
        string recipientEmail,
        string recipientName,
        string periodName,
        string assignedByName,
        string designation,
        string department,
        int kpiCount,
        CancellationToken cancellationToken = default)
    {
        var host = _configuration["Email:SmtpHost"];
        var username = _configuration["Email:Username"];
        var password = _configuration["Email:Password"];
        var fromEmail = _configuration["Email:FromEmail"];

        if (string.IsNullOrWhiteSpace(host) ||
            string.IsNullOrWhiteSpace(username) ||
            string.IsNullOrWhiteSpace(password) ||
            string.IsNullOrWhiteSpace(fromEmail))
        {
            throw new InvalidOperationException(
                "Email SMTP configuration is incomplete. Configure Email:SmtpHost, Email:Username, Email:Password, and Email:FromEmail.");
        }

        var port = _configuration.GetValue("Email:SmtpPort", 587);
        var enableSsl = _configuration.GetValue("Email:EnableSsl", true);
        var fromName = _configuration["Email:FromName"] ?? "Employee Evaluation System";
        var evaluationUrl = _configuration["Email:KpiEvaluationUrl"] ?? "http://localhost:3000/my-kpis";

        var safeRecipientName = WebUtility.HtmlEncode(recipientName);
        var safePeriodName = WebUtility.HtmlEncode(periodName);
        var safeAssignedByName = WebUtility.HtmlEncode(assignedByName);
        var safeDesignation = WebUtility.HtmlEncode(designation);
        var safeDepartment = WebUtility.HtmlEncode(department);
        var safeEvaluationUrl = WebUtility.HtmlEncode(evaluationUrl);

        using var message = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
            Subject = $"KPIs published for {periodName}",
            Body = $"""
                <p>Dear {safeRecipientName},</p>

                <p>Kindly log in to the Employee Evaluation System and complete your
                <strong>self-evaluation</strong> for each assigned KPI.</p>

                <p><strong>KPI Assignment Details:</strong></p>
                <ul>
                    <li><strong>Evaluation Period:</strong> {safePeriodName}</li>
                    <li><strong>Assigned By:</strong> {safeAssignedByName}</li>
                    <li><strong>Designation:</strong> {safeDesignation}</li>
                    <li><strong>Department:</strong> {safeDepartment}</li>
                    <li><strong>Total Assigned KPIs:</strong> {kpiCount}</li>
                </ul>

                <p><strong>KPI Evaluation Link:</strong>
                <a href="{safeEvaluationUrl}">{safeEvaluationUrl}</a></p>

                <p>Please ensure that your self-evaluation is completed within the given timeline.</p>

                <p>Thankyou,<br>
                GD Department.</p>
                """,
            IsBodyHtml = true
        };
        message.To.Add(new MailAddress(recipientEmail, recipientName));

        using var smtpClient = new SmtpClient(host, port)
        {
            EnableSsl = enableSsl,
            UseDefaultCredentials = false,
            Credentials = new NetworkCredential(username, password)
        };

        _logger.LogInformation("Sending KPI published email to {RecipientEmail}", recipientEmail);
        await smtpClient.SendMailAsync(message, cancellationToken);
    }
}

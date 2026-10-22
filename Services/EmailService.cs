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

        using var message = new MailMessage
        {
            From = new MailAddress(fromEmail, fromName),
            Subject = $"KPIs published for {periodName}",
            Body = $"Hello {recipientName},\n\nYour KPI assignment for '{periodName}' has been published. You can now sign in to the Employee Evaluation System and review your KPIs.\n\nRegards,\n{fromName}",
            IsBodyHtml = false
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

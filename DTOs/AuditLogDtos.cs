namespace EmployeeEvaluationSystem.DTOs;

public class AuditLogDto
{
    public int AuditId { get; set; }
    public int UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public int? RecordId { get; set; }
    public DateTime CreatedDate { get; set; }
}

/// <summary>Filter parameters for GET /api/audit-log.</summary>
public class AuditLogSearchDto
{
    public int? UserId { get; set; }
    public string? TableName { get; set; }
    public string? Action { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 50;
}

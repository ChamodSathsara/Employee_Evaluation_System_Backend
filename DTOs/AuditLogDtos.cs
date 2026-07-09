namespace EmployeeEvaluationSystem.DTOs;

public record AuditLogDto(int AuditId, int UserId, string Action, string TableName, int? RecordId, DateTime CreatedDate);

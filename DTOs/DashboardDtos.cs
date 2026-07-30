namespace EmployeeEvaluationSystem.DTOs;

public class EmployeeDashboardDto
{
    public List<KpiAssignmentDto> MyKpis { get; set; } = new();
    public bool HasPendingSelfEvaluation { get; set; }
    public EvaluationResultDto? LatestResult { get; set; }
    public List<NotificationDto> Notifications { get; set; } = new();
}

public class SupervisorDashboardDto
{
    public List<UserDto> Employees { get; set; } = new();
    public List<PendingReviewDto> PendingReviews { get; set; } = new();
    public List<NotificationDto> Notifications { get; set; } = new();
}

public class HodDashboardDto
{
    public List<UserDto> DepartmentEmployees { get; set; } = new();
    public List<PendingReviewDto> PendingReviews { get; set; } = new();
    public List<KpiAssignmentDto> KpiStatus { get; set; } = new();
    public List<NotificationDto> Notifications { get; set; } = new();
}

public class ManagerDashboardDto
{
    public List<PendingManagerReviewDto> PendingApprovals { get; set; } = new();
    public List<HodEvaluationDto> HodEvaluations { get; set; } = new();
    public Dictionary<string, int> GradeDistribution { get; set; } = new();
    public List<NotificationDto> Notifications { get; set; } = new();
}

public class CeoDashboardDto
{
    public List<TopPerformerDto> ManagerPerformance { get; set; } = new();
    public List<RecommendationCandidateDto> PromotionSummary { get; set; } = new();
    public int TotalActiveUsers { get; set; }
    public int TotalDepartments { get; set; }
    public List<NotificationDto> Notifications { get; set; } = new();
}

public class AdminDashboardDto
{
    public int TotalUsers { get; set; }
    public int ActiveUsers { get; set; }
    public int TotalDepartments { get; set; }
    public EvaluationPeriodDto? ActiveEvaluationPeriod { get; set; }
    public List<AuditLogDto> RecentAuditLogs { get; set; } = new();
}

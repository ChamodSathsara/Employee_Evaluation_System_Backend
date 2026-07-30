namespace EmployeeEvaluationSystem.DTOs;

/// <summary>Full individual employee evaluation report for a given period.</summary>
public class EmployeeReportDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public string DesignationName { get; set; } = string.Empty;
    public string PeriodName { get; set; } = string.Empty;
    public decimal KPIScore { get; set; }
    public decimal CompetencyScore { get; set; }
    public decimal AchievementScore { get; set; }
    public decimal AttendancePenalty { get; set; }
    public decimal FinalScore { get; set; }
    public string Grade { get; set; } = string.Empty;
    public List<string> Recommendations { get; set; } = new();
    public string? ManagerDecision { get; set; }
    public string? ManagerComments { get; set; }
}

public class DepartmentReportDto
{
    public int DepartmentId { get; set; }
    public string DepartmentName { get; set; } = string.Empty;
    public int EmployeeCount { get; set; }
    public decimal AverageFinalScore { get; set; }
    public Dictionary<string, int> GradeDistribution { get; set; } = new();
}

public class SummaryReportDto
{
    public int TotalEmployeesEvaluated { get; set; }
    public decimal AverageFinalScore { get; set; }
    public Dictionary<string, int> GradeDistribution { get; set; } = new();
    public List<DepartmentReportDto> ByDepartment { get; set; } = new();
}

public class TopPerformerDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public decimal FinalScore { get; set; }
    public string Grade { get; set; } = string.Empty;
}

public class RecommendationCandidateDto
{
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string DepartmentName { get; set; } = string.Empty;
    public decimal FinalScore { get; set; }
    public string Grade { get; set; } = string.Empty;
    public string? RecommendationNote { get; set; }
}

namespace EmployeeEvaluationSystem.DTOs;

/// <summary>Per-KPI line item, showing Employee / Supervisor / HOD marks side by side.</summary>
public class EvaluationKpiLineDto
{
    public int KPIAssignmentDetailId { get; set; }
    public string KPIName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal WeightPercentage { get; set; }
    public decimal? EmployeeMarks { get; set; }
    public string? EmployeeComments { get; set; }
    public decimal? SupervisorMarks { get; set; }
    public string? SupervisorComments { get; set; }
    public decimal? HodMarks { get; set; }
    public string? HodComments { get; set; }
}

public class EvaluationCompetencyLineDto
{
    public int CompetencyId { get; set; }
    public string CompetencyName { get; set; } = string.Empty;
    public int? Score { get; set; }
    public string? Remarks { get; set; }
}

public class EvaluationAchievementLineDto
{
    public int AchievementId { get; set; }
    public string AchievementTypeName { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public decimal Marks { get; set; }
    public string? Attachment { get; set; }
    public string? Remarks { get; set; }
}

/// <summary>
/// The single combined "evaluation detail" view used by both the Employee Results tab and
/// the Manager/HOD "employee evaluation page": every KPI line with Employee/Supervisor/HOD
/// marks, competency scores, achievements, attendance penalty, final score/grade, and the
/// HOD recommendation/comment - in one response instead of five separate calls.
/// </summary>
public class EvaluationDetailDto
{
    public int AssignmentId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public string? DesignationName { get; set; }
    public int PeriodId { get; set; }
    public string PeriodName { get; set; } = string.Empty;

    public int? SelfEvaluationId { get; set; }
    public string SelfEvaluationStatus { get; set; } = "Not Started";

    public int? SupervisorEvaluationId { get; set; }
    public string? SupervisorEvaluationStatus { get; set; }

    public int? HodEvaluationId { get; set; }
    public string? HodEvaluationStatus { get; set; }

    public List<EvaluationKpiLineDto> KpiLines { get; set; } = new();
    public List<EvaluationCompetencyLineDto> Competencies { get; set; } = new();
    public List<EvaluationAchievementLineDto> Achievements { get; set; } = new();

    public decimal? AttendancePenalty { get; set; }

    public bool IsProcessed { get; set; }
    public int? ResultId { get; set; }
    public decimal? KPIScore { get; set; }
    public decimal? CompetencyScore { get; set; }
    public decimal? AchievementScore { get; set; }
    public decimal? FinalScore { get; set; }
    public string? Grade { get; set; }

    public List<string> RecommendationTypes { get; set; } = new();
    public string? RecommendationNote { get; set; }

    public string? ManagerDecision { get; set; }
    public string? ManagerComments { get; set; }
}

namespace EmployeeEvaluationSystem.DTOs;

public class EvaluationResultDto
{
    public int ResultId { get; set; }
    public int HODEvaluationId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public decimal KPIScore { get; set; }
    public decimal CompetencyScore { get; set; }
    public decimal AchievementScore { get; set; }
    public decimal AttendancePenalty { get; set; }
    public decimal FinalScore { get; set; }
    public string Grade { get; set; } = string.Empty;
    public DateTime ProcessedDate { get; set; }
}

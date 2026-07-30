using System.ComponentModel.DataAnnotations;

namespace EmployeeEvaluationSystem.DTOs;

public class ManagerReviewDto
{
    public int ManagerReviewId { get; set; }
    public int ResultId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int ManagerId { get; set; }
    public string ManagerName { get; set; } = string.Empty;
    public string? Comments { get; set; }
    public string Decision { get; set; } = string.Empty;
    public DateTime ReviewDate { get; set; }
}

public class CreateManagerReviewDto
{
    [Required]
    public int ResultId { get; set; }

    [MaxLength(500)]
    public string? Comments { get; set; }
}

public class UpdateManagerReviewDto
{
    [MaxLength(500)]
    public string? Comments { get; set; }
}

public class ManagerDecisionDto
{
    [MaxLength(500)]
    public string? Comments { get; set; }
}

public class ReturnEvaluationDto
{
    [Required, MaxLength(500)]
    public string Reason { get; set; } = string.Empty;
}

/// <summary>Summary row for the Manager's pending-approval queue.</summary>
public class PendingManagerReviewDto
{
    public int ResultId { get; set; }
    public int HODEvaluationId { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public decimal FinalScore { get; set; }
    public string Grade { get; set; } = string.Empty;
    public DateTime ProcessedDate { get; set; }
}

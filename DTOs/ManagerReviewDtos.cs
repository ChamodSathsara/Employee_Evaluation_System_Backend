namespace EmployeeEvaluationSystem.DTOs;

public record ManagerReviewDto(int ManagerReviewId, int ResultId, int ManagerId, string? Comments, string Decision, DateTime ReviewDate);
public record SubmitManagerReviewDto(string? Comments, string Decision);

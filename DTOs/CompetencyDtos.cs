namespace EmployeeEvaluationSystem.DTOs;

public record CompetencyDto(int CompetencyId, string Name, string? Description);
public record CompetencyEvaluationDto(int CompetencyEvaluationId, int HODEvaluationId, int CompetencyId, int Score, string? Remarks);
public record SubmitCompetencyEvaluationDto(int CompetencyId, int Score, string? Remarks);

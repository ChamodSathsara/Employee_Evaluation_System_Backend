namespace EmployeeEvaluationSystem.DTOs;

public record HodEvaluationDetailDto(int DetailId, int KPIAssignmentDetailId, decimal Marks, string? Comments);
public record HodEvaluationDto(int HODEvaluationId, int SelfEvaluationId, int HODId, string Status, List<HodEvaluationDetailDto> Details);

public record SubmitHodEvaluationDto(List<HodEvaluationDetailDto> Details);

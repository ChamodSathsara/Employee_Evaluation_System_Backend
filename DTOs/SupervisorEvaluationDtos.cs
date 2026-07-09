namespace EmployeeEvaluationSystem.DTOs;

public record SupervisorEvaluationDetailDto(int DetailId, int KPIAssignmentDetailId, decimal Marks, string? Comments);
public record SupervisorEvaluationDto(int SupervisorEvaluationId, int SelfEvaluationId, int SupervisorId, string Status, List<SupervisorEvaluationDetailDto> Details);

public record SubmitSupervisorEvaluationDto(List<SupervisorEvaluationDetailDto> Details);

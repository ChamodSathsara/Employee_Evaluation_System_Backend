namespace EmployeeEvaluationSystem.DTOs;

public record EmployeeEvaluationDetailDto(int DetailId, int KPIAssignmentDetailId, decimal EmployeeMarks, string? Comments);
public record EmployeeEvaluationDto(int SelfEvaluationId, int AssignmentId, int EmployeeId, string Status, List<EmployeeEvaluationDetailDto> Details);

public record SubmitEmployeeEvaluationDto(List<EmployeeEvaluationDetailDto> Details);

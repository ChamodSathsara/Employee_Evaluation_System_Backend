namespace EmployeeEvaluationSystem.DTOs;

public record DepartmentDto(int DepartmentId, string DepartmentName, string? Description);
public record CreateDepartmentDto(string DepartmentName, string? Description);

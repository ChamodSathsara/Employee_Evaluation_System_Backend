using EmployeeEvaluationSystem.DTOs;

namespace EmployeeEvaluationSystem.Services;

public interface IDashboardService
{
    Task<EmployeeDashboardDto> GetEmployeeDashboardAsync(int employeeId);
    Task<SupervisorDashboardDto> GetSupervisorDashboardAsync(int supervisorId);
    Task<HodDashboardDto> GetHodDashboardAsync(int hodId);
    Task<ManagerDashboardDto> GetManagerDashboardAsync(int managerId);
    Task<CeoDashboardDto> GetCeoDashboardAsync(int callerId);
    Task<AdminDashboardDto> GetAdminDashboardAsync();
}

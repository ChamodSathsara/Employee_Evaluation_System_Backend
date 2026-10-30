using EmployeeEvaluationSystem.DTOs;

namespace EmployeeEvaluationSystem.Services;

public interface ICompanyService
{
    Task<List<CompanyDto>> GetAllAsync();
    Task<CompanyDto> GetByIdAsync(int companyId);
    Task<CompanyDto> CreateAsync(CreateCompanyDto request);
    Task<CompanyDto> UpdateAsync(int companyId, UpdateCompanyDto request);
}

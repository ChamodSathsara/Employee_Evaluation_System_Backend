using AutoMapper;
using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Models;
using EmployeeEvaluationSystem.Repositories;

namespace EmployeeEvaluationSystem.Services;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public CompanyService(
        ICompanyRepository companyRepository,
        IAuditLogRepository auditLogRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _companyRepository = companyRepository;
        _auditLogRepository = auditLogRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<List<CompanyDto>> GetAllAsync()
    {
        var companies = await _companyRepository.ListAllAsync();
        var result = _mapper.Map<List<CompanyDto>>(companies);
        foreach (var company in result)
        {
            company.UserCount = await _companyRepository.CountUsersAsync(company.CompanyId);
        }
        return result;
    }

    public async Task<CompanyDto> GetByIdAsync(int companyId)
    {
        var company = await _companyRepository.GetByIdAsync(companyId)
            ?? throw new NotFoundException("Company", companyId);
        var result = _mapper.Map<CompanyDto>(company);
        result.UserCount = await _companyRepository.CountUsersAsync(companyId);
        return result;
    }

    public async Task<CompanyDto> CreateAsync(CreateCompanyDto request)
    {
        if (!_currentUserService.IsInRole(RoleNames.Admin))
        {
            throw new ForbiddenException("Only Admin can manage companies.");
        }

        var companyName = request.CompanyName.Trim();
        if (await _companyRepository.ExistsByNameAsync(companyName))
        {
            throw new ConflictException($"A company named '{companyName}' already exists.");
        }

        var company = new Company { CompanyName = companyName };
        await _companyRepository.AddAsync(company);
        await _companyRepository.SaveChangesAsync();

        await _auditLogRepository.AddAsync(new AuditLog
        {
            UserId = _currentUserService.UserId ?? 0,
            Action = "Create",
            TableName = "Companies",
            RecordId = company.CompanyId,
            CreatedDate = DateTime.UtcNow
        });
        await _auditLogRepository.SaveChangesAsync();

        return _mapper.Map<CompanyDto>(company);
    }
}

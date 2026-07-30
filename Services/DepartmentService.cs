using AutoMapper;
using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Models;
using EmployeeEvaluationSystem.Repositories;

namespace EmployeeEvaluationSystem.Services;

/// <summary>Admin-only CRUD for departments.</summary>
public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public DepartmentService(
        IDepartmentRepository departmentRepository,
        IAuditLogRepository auditLogRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _departmentRepository = departmentRepository;
        _auditLogRepository = auditLogRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<List<DepartmentDto>> GetAllAsync()
    {
        var departments = await _departmentRepository.ListAllAsync();
        var dtos = _mapper.Map<List<DepartmentDto>>(departments);

        foreach (var dto in dtos)
        {
            dto.UserCount = await _departmentRepository.CountUsersAsync(dto.DepartmentId);
        }

        return dtos;
    }

    public async Task<DepartmentDto> GetByIdAsync(int departmentId)
    {
        var department = await _departmentRepository.GetByIdAsync(departmentId)
            ?? throw new NotFoundException("Department", departmentId);

        var dto = _mapper.Map<DepartmentDto>(department);
        dto.UserCount = await _departmentRepository.CountUsersAsync(departmentId);
        return dto;
    }

    public async Task<DepartmentDto> CreateAsync(CreateDepartmentDto request)
    {
        EnsureCallerIsAdmin();

        if (await _departmentRepository.ExistsByNameAsync(request.DepartmentName))
        {
            throw new ConflictException($"A department named '{request.DepartmentName}' already exists.");
        }

        var department = _mapper.Map<Department>(request);
        await _departmentRepository.AddAsync(department);
        await LogAuditAsync("Create", "Departments", department.DepartmentId);
        await _departmentRepository.SaveChangesAsync();

        return _mapper.Map<DepartmentDto>(department);
    }

    public async Task<DepartmentDto> UpdateAsync(int departmentId, UpdateDepartmentDto request)
    {
        EnsureCallerIsAdmin();

        var department = await _departmentRepository.GetByIdAsync(departmentId)
            ?? throw new NotFoundException("Department", departmentId);

        if (await _departmentRepository.ExistsByNameAsync(request.DepartmentName, excludeDepartmentId: departmentId))
        {
            throw new ConflictException($"A department named '{request.DepartmentName}' already exists.");
        }

        _mapper.Map(request, department);
        await _departmentRepository.UpdateAsync(department);
        await LogAuditAsync("Update", "Departments", departmentId);
        await _departmentRepository.SaveChangesAsync();

        return _mapper.Map<DepartmentDto>(department);
    }

    public async Task DeleteAsync(int departmentId)
    {
        EnsureCallerIsAdmin();

        var department = await _departmentRepository.GetByIdAsync(departmentId)
            ?? throw new NotFoundException("Department", departmentId);

        var userCount = await _departmentRepository.CountUsersAsync(departmentId);
        if (userCount > 0)
        {
            throw new ConflictException($"This department has {userCount} user(s) assigned and cannot be deleted.");
        }

        await _departmentRepository.DeleteAsync(department);
        await LogAuditAsync("Delete", "Departments", departmentId);
        await _departmentRepository.SaveChangesAsync();
    }

    private void EnsureCallerIsAdmin()
    {
        if (!_currentUserService.IsInRole(RoleNames.Admin))
        {
            throw new ForbiddenException("Only Admin can manage departments.");
        }
    }

    private async Task LogAuditAsync(string action, string tableName, int? recordId)
    {
        await _auditLogRepository.AddAsync(new AuditLog
        {
            UserId = _currentUserService.UserId ?? 0,
            Action = action,
            TableName = tableName,
            RecordId = recordId,
            CreatedDate = DateTime.UtcNow
        });
    }
}

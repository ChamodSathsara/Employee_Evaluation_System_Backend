using AutoMapper;
using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Models;
using EmployeeEvaluationSystem.Repositories;

namespace EmployeeEvaluationSystem.Services;

/// <summary>Admin-only CRUD for the fixed set of system roles (Admin, CEO, Manager, HOD, Supervisor, Employee).</summary>
public class RoleService : IRoleService
{
    private readonly IRoleRepository _roleRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public RoleService(
        IRoleRepository roleRepository,
        IAuditLogRepository auditLogRepository,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _roleRepository = roleRepository;
        _auditLogRepository = auditLogRepository;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<List<RoleDto>> GetAllAsync()
    {
        var roles = await _roleRepository.ListAllAsync();
        var dtos = _mapper.Map<List<RoleDto>>(roles);

        foreach (var dto in dtos)
        {
            dto.UserCount = await _roleRepository.CountUsersAsync(dto.RoleId);
        }

        return dtos;
    }

    public async Task<RoleDto> GetByIdAsync(int roleId)
    {
        var role = await _roleRepository.GetByIdAsync(roleId)
            ?? throw new NotFoundException("Role", roleId);

        var dto = _mapper.Map<RoleDto>(role);
        dto.UserCount = await _roleRepository.CountUsersAsync(roleId);
        return dto;
    }

    public async Task<RoleDto> CreateAsync(CreateRoleDto request)
    {
        EnsureCallerIsAdmin();

        if (await _roleRepository.ExistsByNameAsync(request.RoleName))
        {
            throw new ConflictException($"A role named '{request.RoleName}' already exists.");
        }

        var role = _mapper.Map<Role>(request);
        await _roleRepository.AddAsync(role);
        await LogAuditAsync("Create", "Roles", role.RoleId);
        await _roleRepository.SaveChangesAsync();

        return _mapper.Map<RoleDto>(role);
    }

    public async Task<RoleDto> UpdateAsync(int roleId, UpdateRoleDto request)
    {
        EnsureCallerIsAdmin();

        var role = await _roleRepository.GetByIdAsync(roleId)
            ?? throw new NotFoundException("Role", roleId);

        if (await _roleRepository.ExistsByNameAsync(request.RoleName, excludeRoleId: roleId))
        {
            throw new ConflictException($"A role named '{request.RoleName}' already exists.");
        }

        _mapper.Map(request, role);
        await _roleRepository.UpdateAsync(role);
        await LogAuditAsync("Update", "Roles", roleId);
        await _roleRepository.SaveChangesAsync();

        return _mapper.Map<RoleDto>(role);
    }

    public async Task DeleteAsync(int roleId)
    {
        EnsureCallerIsAdmin();

        var role = await _roleRepository.GetByIdAsync(roleId)
            ?? throw new NotFoundException("Role", roleId);

        var userCount = await _roleRepository.CountUsersAsync(roleId);
        if (userCount > 0)
        {
            throw new ConflictException($"This role is assigned to {userCount} user(s) and cannot be deleted.");
        }

        await _roleRepository.DeleteAsync(role);
        await LogAuditAsync("Delete", "Roles", roleId);
        await _roleRepository.SaveChangesAsync();
    }

    private void EnsureCallerIsAdmin()
    {
        if (!_currentUserService.IsInRole(RoleNames.Admin))
        {
            throw new ForbiddenException("Only Admin can manage roles.");
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

using AutoMapper;
using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Models;
using EmployeeEvaluationSystem.Repositories;
using Microsoft.EntityFrameworkCore;

namespace EmployeeEvaluationSystem.Services;

/// <summary>
/// Implements CRUD for Users and enforces the organizational hierarchy rules:
/// Admin/CEO have no manager; Manager reports to a CEO; HOD reports to a Manager;
/// Supervisor reports to an HOD; Employee reports to a Supervisor or an HOD.
/// Only Admin may create, update, deactivate, or delete users.
/// </summary>
public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly ICompanyRepository _companyRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IDesignationRepository _designationRepository;
    private readonly IKPIAssignmentRepository _kpiAssignmentRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ICurrentUserService _currentUserService;
    private readonly IMapper _mapper;

    public UserService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        ICompanyRepository companyRepository,
        IDepartmentRepository departmentRepository,
        IDesignationRepository designationRepository,
        IKPIAssignmentRepository kpiAssignmentRepository,
        IAuditLogRepository auditLogRepository,
        IPasswordHasher passwordHasher,
        ICurrentUserService currentUserService,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _companyRepository = companyRepository;
        _departmentRepository = departmentRepository;
        _designationRepository = designationRepository;
        _kpiAssignmentRepository = kpiAssignmentRepository;
        _auditLogRepository = auditLogRepository;
        _passwordHasher = passwordHasher;
        _currentUserService = currentUserService;
        _mapper = mapper;
    }

    public async Task<PagedResultDto<UserDto>> GetAllAsync(UserSearchDto search)
    {
        var query = _userRepository.QueryWithDetails();

        if (!string.IsNullOrWhiteSpace(search.Keyword))
        {
            var keyword = search.Keyword.Trim().ToLower();
            query = query.Where(u =>
                u.FirstName.ToLower().Contains(keyword) ||
                u.LastName.ToLower().Contains(keyword) ||
                u.Email.ToLower().Contains(keyword) ||
                u.EmployeeNo.ToLower().Contains(keyword));
        }

        if (search.RoleId.HasValue)
        {
            query = query.Where(u => u.RoleId == search.RoleId.Value);
        }

        if (search.CompanyId.HasValue)
        {
            query = query.Where(u => u.CompanyId == search.CompanyId.Value);
        }

        if (search.DepartmentId.HasValue)
        {
            query = query.Where(u => u.DepartmentId == search.DepartmentId.Value);
        }


        if (search.DepartmentId.HasValue)
        {
            query = query.Where(u => u.DepartmentId == search.DepartmentId.Value);
        }

        if (search.IsActive.HasValue)
        {
            query = query.Where(u => u.IsActive == search.IsActive.Value);
        }

        var pageNumber = search.PageNumber < 1 ? 1 : search.PageNumber;
        var pageSize = search.PageSize is < 1 or > 100 ? 20 : search.PageSize;

        var totalCount = await query.CountAsync();

        var users = await query
            .Include(u => u.ReportedSupervisor)
            .Include(u => u.ReportedHod)
            .OrderBy(u => u.FirstName).ThenBy(u => u.LastName)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
        var userDtos = _mapper.Map<List<UserDto>>(users);

        return new PagedResultDto<UserDto>
        {
            Items = userDtos,
            TotalCount = totalCount,
            PageNumber = pageNumber,
            PageSize = pageSize
        };
    }

    public async Task<UserDto> GetByIdAsync(int userId)
    {
        var user = await _userRepository.GetByIdWithDetailsAsync(userId)
            ?? throw new NotFoundException("User", userId);

        return _mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> CreateUserAsync(CreateUserDto request)
    {
        EnsureCallerIsAdmin();

        if (await _userRepository.ExistsByEmailAsync(request.Email))
        {
            throw new ConflictException($"A user with email '{request.Email}' already exists.");
        }

        if (await _userRepository.ExistsByEmployeeNoAsync(request.EmployeeNo))
        {
            throw new ConflictException($"A user with employee number '{request.EmployeeNo}' already exists.");
        }

        var role = await _roleRepository.GetByIdAsync(request.RoleId)
            ?? throw new NotFoundException("Role", request.RoleId);

        if (!await _companyRepository.ExistsAsync(request.CompanyId))
        {
            throw new NotFoundException("Company", request.CompanyId);
        }

        if (request.DepartmentId.HasValue && !await _departmentRepository.ExistsAsync(request.DepartmentId.Value))
        {
            throw new NotFoundException("Department", request.DepartmentId.Value);
        }

        if (request.DesignationId.HasValue && !await _designationRepository.ExistsAsync(request.DesignationId.Value))
        {
            throw new NotFoundException("Designation", request.DesignationId.Value);
        }

        await ValidateReportingAssignmentsAsync(role.RoleName, request.ReportedSupervisorId, request.ReportedHodId);

        var user = _mapper.Map<User>(request);
        user.PasswordHash = _passwordHasher.Hash(request.Password);
        user.IsActive = true;
        user.CreatedDate = DateTime.UtcNow;

        await _userRepository.AddAsync(user);
        await LogAuditAsync("Create", "Users", user.UserId);
        await _userRepository.SaveChangesAsync();

        var created = await _userRepository.GetByIdWithDetailsAsync(user.UserId);
        return _mapper.Map<UserDto>(created!);
    }

    public async Task<UserDto> UpdateUserAsync(int userId, UpdateUserDto request)
    {
        EnsureCallerIsAdmin();

        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

        if (await _userRepository.ExistsByEmailAsync(request.Email, excludeUserId: userId))
        {
            throw new ConflictException($"A user with email '{request.Email}' already exists.");
        }

        var role = await _roleRepository.GetByIdAsync(request.RoleId)
            ?? throw new NotFoundException("Role", request.RoleId);

        if (!await _companyRepository.ExistsAsync(request.CompanyId))
        {
            throw new NotFoundException("Company", request.CompanyId);
        }

        if (request.DepartmentId.HasValue && !await _departmentRepository.ExistsAsync(request.DepartmentId.Value))
        {
            throw new NotFoundException("Department", request.DepartmentId.Value);
        }

        if (request.DesignationId.HasValue && !await _designationRepository.ExistsAsync(request.DesignationId.Value))
        {
            throw new NotFoundException("Designation", request.DesignationId.Value);
        }

        if (request.ReportedSupervisorId == userId || request.ReportedHodId == userId)
        {
            throw new ValidationAppException("A user cannot report to themselves.");
        }

        await ValidateReportingAssignmentsAsync(role.RoleName, request.ReportedSupervisorId, request.ReportedHodId);

        _mapper.Map(request, user);

        await _userRepository.UpdateAsync(user);
        await LogAuditAsync("Update", "Users", userId);
        await _userRepository.SaveChangesAsync();

        var updated = await _userRepository.GetByIdWithDetailsAsync(userId);
        return _mapper.Map<UserDto>(updated!);
    }

    public async Task DeleteUserAsync(int userId)
    {
        EnsureCallerIsAdmin();

        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

        var directReports = await _userRepository.GetDirectReportsAsync(userId);
        if (directReports.Count > 0)
        {
            throw new ConflictException(
                "This user has direct reports and cannot be deleted. Reassign their reports or deactivate the account instead.");
        }

        var hasKpiInvolvement = await _kpiAssignmentRepository.ExistsForUserAsync(userId);
        if (hasKpiInvolvement)
        {
            throw new ConflictException(
                "This user has associated KPI or evaluation records and cannot be deleted. Deactivate the account instead.");
        }

        await _userRepository.DeleteAsync(user);
        await LogAuditAsync("Delete", "Users", userId);
        await _userRepository.SaveChangesAsync();
    }

    public async Task<UserDto> UpdateStatusAsync(int userId, UpdateUserStatusDto request)
    {
        EnsureCallerIsAdmin();

        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

        user.IsActive = request.IsActive;

        // Deactivating a user also revokes any outstanding refresh token immediately.
        if (!request.IsActive)
        {
            user.RefreshTokenHash = null;
            user.RefreshTokenExpiryTime = null;
        }

        await _userRepository.UpdateAsync(user);
        await LogAuditAsync("Update", "Users", userId);
        await _userRepository.SaveChangesAsync();

        var updated = await _userRepository.GetByIdWithDetailsAsync(userId);
        return _mapper.Map<UserDto>(updated!);
    }

    public async Task<List<UserDto>> GetDirectReportsAsync(int managerUserId)
    {
        if (await _userRepository.GetByIdAsync(managerUserId) is null)
        {
            throw new NotFoundException("User", managerUserId);
        }

        var reports = await _userRepository.GetDirectReportsAsync(managerUserId);
        return _mapper.Map<List<UserDto>>(reports);
    }

    public async Task<List<UserDto>> GetByRoleAsync(int roleId)
    {
        if (await _roleRepository.GetByIdAsync(roleId) is null)
        {
            throw new NotFoundException("Role", roleId);
        }

        var users = await _userRepository.GetByRoleIdAsync(roleId);
        return _mapper.Map<List<UserDto>>(users);
    }

    public async Task<List<UserDto>> GetByDepartmentAsync(int departmentId)
    {
        if (!await _departmentRepository.ExistsAsync(departmentId))
        {
            throw new NotFoundException("Department", departmentId);
        }

        var users = await _userRepository.GetByDepartmentIdAsync(departmentId);
        return _mapper.Map<List<UserDto>>(users);
    }

    public async Task<List<UserDto>> GetManagementUsersAsync()
    {
        var managementRoles = new[] { RoleNames.CEO, RoleNames.Manager, RoleNames.HOD, RoleNames.Supervisor };
        var users = await _userRepository.GetByRoleNamesAsync(managementRoles);
        return _mapper.Map<List<UserDto>>(users);
    }

    // ---------- private helpers ----------

    private void EnsureCallerIsAdmin()
    {
        // Defense-in-depth: controllers already carry [Authorize(Roles = RoleNames.Admin)],
        // this re-validates at the service layer per the "Services validate permissions" rule.
        if (!_currentUserService.IsInRole(RoleNames.Admin))
        {
            throw new ForbiddenException("Only Admin can manage users.");
        }
    }

    /// <summary>
    /// Enforces the fixed organizational hierarchy:
    /// Admin/CEO -> no manager. Manager -> reports to a CEO. HOD -> reports to a Manager.
    /// Supervisor -> reports to an HOD. Employee -> reports to a Supervisor or an HOD.
    /// </summary>
    private async Task ValidateReportingAssignmentsAsync(
        string roleName,
        int? reportedSupervisorId,
        int? reportedHodId)
    {
        if (roleName is RoleNames.Admin or RoleNames.CEO)
        {
            if (reportedSupervisorId.HasValue || reportedHodId.HasValue)
            {
                throw new ValidationAppException($"A user with role '{roleName}' must not have a reported supervisor or HOD.");
            }
            return;
        }

        if (roleName == RoleNames.Employee && !reportedSupervisorId.HasValue && !reportedHodId.HasValue)
        {
            throw new ValidationAppException(
                "An Employee must have at least one of ReportedSupervisorId or ReportedHodId.");
        }

        if (roleName == RoleNames.Supervisor && !reportedHodId.HasValue)
        {
            throw new ValidationAppException("A Supervisor must have a ReportedHodId.");
        }

        if (reportedSupervisorId.HasValue)
        {
            var supervisor = await _userRepository.GetByIdWithDetailsAsync(reportedSupervisorId.Value)
                ?? throw new NotFoundException("Reported Supervisor", reportedSupervisorId.Value);
            if (supervisor.Role.RoleName != RoleNames.Supervisor)
            {
                throw new ValidationAppException("ReportedSupervisorId must reference a user with the Supervisor role.");
            }
        }

        if (reportedHodId.HasValue)
        {
            var hod = await _userRepository.GetByIdWithDetailsAsync(reportedHodId.Value)
                ?? throw new NotFoundException("Reported HOD", reportedHodId.Value);
            if ((roleName is RoleNames.Employee or RoleNames.Supervisor) && hod.Role.RoleName != RoleNames.HOD)
            {
                throw new ValidationAppException("ReportedHodId must reference a user with the HOD role.");
            }
        }
    }

    private async Task LogAuditAsync(string action, string tableName, int? recordId)
    {
        await _auditLogRepository.AddAsync(new AuditLog
        {
            UserId = _currentUserService.UserId ?? recordId ?? 0,
            Action = action,
            TableName = tableName,
            RecordId = recordId,
            CreatedDate = DateTime.UtcNow
        });
    }
}

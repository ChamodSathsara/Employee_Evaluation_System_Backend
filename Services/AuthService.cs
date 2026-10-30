using AutoMapper;
using EmployeeEvaluationSystem.DTOs;
using EmployeeEvaluationSystem.Helpers;
using EmployeeEvaluationSystem.Models;
using EmployeeEvaluationSystem.Repositories;
using Microsoft.Extensions.Configuration;

namespace EmployeeEvaluationSystem.Services;

/// <summary>
/// Implements login, registration, JWT refresh-token rotation, logout, password change,
/// and profile retrieval. All business rules for authentication live here; the controller
/// only translates HTTP requests into calls against this service.
/// </summary>
public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRoleRepository _roleRepository;
    private readonly IDepartmentRepository _departmentRepository;
    private readonly IDesignationRepository _designationRepository;
    private readonly IAuditLogRepository _auditLogRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenHelper _jwtTokenHelper;
    private readonly ICurrentUserService _currentUserService;
    private readonly IConfiguration _configuration;
    private readonly IMapper _mapper;

    public AuthService(
        IUserRepository userRepository,
        IRoleRepository roleRepository,
        IDepartmentRepository departmentRepository,
        IDesignationRepository designationRepository,
        IAuditLogRepository auditLogRepository,
        IPasswordHasher passwordHasher,
        IJwtTokenHelper jwtTokenHelper,
        ICurrentUserService currentUserService,
        IConfiguration configuration,
        IMapper mapper)
    {
        _userRepository = userRepository;
        _roleRepository = roleRepository;
        _departmentRepository = departmentRepository;
        _designationRepository = designationRepository;
        _auditLogRepository = auditLogRepository;
        _passwordHasher = passwordHasher;
        _jwtTokenHelper = jwtTokenHelper;
        _currentUserService = currentUserService;
        _configuration = configuration;
        _mapper = mapper;
    }

    public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
    {
        var user = await _userRepository.GetByEmailAsync(request.Email);
        if (user is null || !_passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAppException("Invalid email or password.");
        }

        if (!user.IsActive)
        {
            throw new ForbiddenException("This account has been deactivated. Please contact your administrator.");
        }

        var response = await IssueTokensAsync(user);

        await LogAuditAsync(user.UserId, "Login", "Users", user.UserId);
        await _userRepository.SaveChangesAsync();

        return response;
    }

    public async Task<UserProfileDto> RegisterAsync(RegisterRequestDto request)
    {
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

        if (role.RoleName == RoleNames.Employee &&
            !request.ReportedSupervisorId.HasValue && !request.ReportedHodId.HasValue)
        {
            throw new ValidationAppException(
                "An Employee must have at least one of ReportedSupervisorId or ReportedHodId.");
        }

        if (request.ReportedSupervisorId.HasValue)
        {
            var supervisor = await _userRepository.GetByIdWithDetailsAsync(request.ReportedSupervisorId.Value)
                ?? throw new NotFoundException("Reported Supervisor", request.ReportedSupervisorId.Value);
            if (supervisor.Role.RoleName != RoleNames.Supervisor)
            {
                throw new ValidationAppException("ReportedSupervisorId must reference a user with the Supervisor role.");
            }
        }

        if (request.ReportedHodId.HasValue)
        {
            var hod = await _userRepository.GetByIdWithDetailsAsync(request.ReportedHodId.Value)
                ?? throw new NotFoundException("Reported HOD", request.ReportedHodId.Value);
            if (role.RoleName == RoleNames.Employee && hod.Role.RoleName != RoleNames.HOD)
            {
                throw new ValidationAppException("ReportedHodId must reference a user with the HOD role.");
            }
        }

        if (request.DepartmentId.HasValue && !await _departmentRepository.ExistsAsync(request.DepartmentId.Value))
        {
            throw new NotFoundException("Department", request.DepartmentId.Value);
        }

        if (request.DesignationId.HasValue && !await _designationRepository.ExistsAsync(request.DesignationId.Value))
        {
            throw new NotFoundException("Designation", request.DesignationId.Value);
        }

        var user = new User
        {
            EmployeeNo = request.EmployeeNo,
            FirstName = request.FirstName,
            LastName = request.LastName,
            Email = request.Email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            RoleId = request.RoleId,
            DepartmentId = request.DepartmentId,
            DesignationId = request.DesignationId,
            ReportedSupervisorId = request.ReportedSupervisorId,
            ReportedHodId = request.ReportedHodId,
            IsActive = true,
            CreatedDate = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user);
        await LogAuditAsync(_currentUserService.UserId ?? user.UserId, "Create", "Users", user.UserId);
        await _userRepository.SaveChangesAsync();

        var created = await _userRepository.GetByIdWithDetailsAsync(user.UserId);
        return _mapper.Map<UserProfileDto>(created!);
    }

    public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
    {
        var principal = _jwtTokenHelper.GetPrincipalFromExpiredToken(request.AccessToken);
        var userIdClaim = principal?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

        if (userIdClaim is null || !int.TryParse(userIdClaim, out var userId))
        {
            throw new UnauthorizedAppException("The provided access token is invalid.");
        }

        var user = await _userRepository.GetByIdWithDetailsAsync(userId)
            ?? throw new UnauthorizedAppException("The provided access token is invalid.");

        var incomingHash = _jwtTokenHelper.HashRefreshToken(request.RefreshToken);

        if (user.RefreshTokenHash is null ||
            user.RefreshTokenHash != incomingHash ||
            user.RefreshTokenExpiryTime is null ||
            user.RefreshTokenExpiryTime < DateTime.UtcNow)
        {
            throw new UnauthorizedAppException("The refresh token is invalid or has expired. Please log in again.");
        }

        if (!user.IsActive)
        {
            throw new ForbiddenException("This account has been deactivated. Please contact your administrator.");
        }

        var response = await IssueTokensAsync(user);
        await _userRepository.SaveChangesAsync();

        return response;
    }

    public async Task LogoutAsync(int userId)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

        user.RefreshTokenHash = null;
        user.RefreshTokenExpiryTime = null;

        await _userRepository.UpdateAsync(user);
        await LogAuditAsync(userId, "Logout", "Users", userId);
        await _userRepository.SaveChangesAsync();
    }

    public async Task ChangePasswordAsync(int userId, ChangePasswordRequestDto request)
    {
        var user = await _userRepository.GetByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

        if (!_passwordHasher.Verify(request.CurrentPassword, user.PasswordHash))
        {
            throw new ValidationAppException("The current password you entered is incorrect.");
        }

        user.PasswordHash = _passwordHasher.Hash(request.NewPassword);

        // Revoke any outstanding refresh token so other sessions must re-authenticate.
        user.RefreshTokenHash = null;
        user.RefreshTokenExpiryTime = null;

        await _userRepository.UpdateAsync(user);
        await LogAuditAsync(userId, "Update", "Users", userId);
        await _userRepository.SaveChangesAsync();
    }

    public async Task<UserProfileDto> GetProfileAsync(int userId)
    {
        var user = await _userRepository.GetByIdWithDetailsAsync(userId)
            ?? throw new NotFoundException("User", userId);

        return _mapper.Map<UserProfileDto>(user);
    }

    // ---------- private helpers ----------

    private async Task<LoginResponseDto> IssueTokensAsync(User user)
    {
        var accessToken = _jwtTokenHelper.GenerateAccessToken(
            user.UserId, user.Email, user.Role.RoleName,
            user.ReportedSupervisorId ?? user.ReportedHodId, user.DepartmentId);

        var refreshToken = _jwtTokenHelper.GenerateRefreshToken();
        var refreshTokenDays = int.Parse(_configuration["Jwt:RefreshTokenExpiryDays"] ?? "7");

        user.RefreshTokenHash = _jwtTokenHelper.HashRefreshToken(refreshToken);
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(refreshTokenDays);

        await _userRepository.UpdateAsync(user);

        var accessTokenMinutes = int.Parse(_configuration["Jwt:AccessTokenExpiryMinutes"] ?? "30");

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAtUtc = DateTime.UtcNow.AddMinutes(accessTokenMinutes),
            User = _mapper.Map<UserProfileDto>(user)
        };
    }

    private async Task LogAuditAsync(int userId, string action, string tableName, int? recordId)
    {
        await _auditLogRepository.AddAsync(new AuditLog
        {
            UserId = userId,
            Action = action,
            TableName = tableName,
            RecordId = recordId,
            CreatedDate = DateTime.UtcNow
        });
    }

}

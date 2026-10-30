using EmployeeEvaluationSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace EmployeeEvaluationSystem.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<Company> Companies => Set<Company>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<Designation> Designations => Set<Designation>();
    public DbSet<EvaluationPeriod> EvaluationPeriods => Set<EvaluationPeriod>();
    public DbSet<KPIAssignment> KpiAssignments => Set<KPIAssignment>();
    public DbSet<KPIAssignmentDetail> KpiAssignmentDetails => Set<KPIAssignmentDetail>();
    public DbSet<EmployeeEvaluation> EmployeeEvaluations => Set<EmployeeEvaluation>();
    public DbSet<EmployeeEvaluationDetail> EmployeeEvaluationDetails => Set<EmployeeEvaluationDetail>();
    public DbSet<SupervisorEvaluation> SupervisorEvaluations => Set<SupervisorEvaluation>();
    public DbSet<SupervisorEvaluationDetail> SupervisorEvaluationDetails => Set<SupervisorEvaluationDetail>();
    public DbSet<HODEvaluation> HodEvaluations => Set<HODEvaluation>();
    public DbSet<HODEvaluationDetail> HodEvaluationDetails => Set<HODEvaluationDetail>();
    public DbSet<Competency> Competencies => Set<Competency>();
    public DbSet<CompetencyEvaluation> CompetencyEvaluations => Set<CompetencyEvaluation>();
    public DbSet<AchievementType> AchievementTypes => Set<AchievementType>();
    public DbSet<EmployeeAchievement> EmployeeAchievements => Set<EmployeeAchievement>();
    public DbSet<AttendanceSummary> AttendanceSummaries => Set<AttendanceSummary>();
    public DbSet<EvaluationResult> EvaluationResults => Set<EvaluationResult>();
    public DbSet<Recommendation> Recommendations => Set<Recommendation>();
    public DbSet<ManagerReview> ManagerReviews => Set<ManagerReview>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Applies every IEntityTypeConfiguration<T> in the Configurations/ folder
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}

namespace EmployeeEvaluationSystem.Helpers;

public interface IPasswordHasher
{
    /// <summary>Hashes a plain-text password using BCrypt with a per-hash salt.</summary>
    string Hash(string password);

    /// <summary>Verifies a plain-text password against a previously computed BCrypt hash.</summary>
    bool Verify(string password, string passwordHash);
}

using SaaSify.Domain.Common;
using SaaSify.Domain.Enums;
using SaaSify.Domain.Exceptions;

namespace SaaSify.Domain.Entities;

public class User : Entity
{
    public string Email { get; private set; } = null!;
    public string PasswordHash { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public UserStatus Status { get; private set; }

    private readonly List<Project> _projects = [];
    public IReadOnlyCollection<Project> Projects => _projects.AsReadOnly();

    // Constructor para EF Core
    private User() { }

    public static User Create(string email, string passwordHash, string name)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new DomainException("Email is required.");

        if (string.IsNullOrWhiteSpace(passwordHash))
            throw new DomainException("Password hash is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name is required.");

        return new User
        {
            Email = email.Trim().ToLowerInvariant(),
            PasswordHash = passwordHash,
            Name = name.Trim(),
            Status = UserStatus.Active
        };
    }

    public void Suspend() => Status = UserStatus.Suspended;

    public void Activate() => Status = UserStatus.Active;

    // Soft delete — nunca se borra físicamente
    public void Delete() => Status = UserStatus.Deleted;

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Name is required.");

        Name = name.Trim();
        MarkAsUpdated(); // ← registra cuándo fue modificado
    }
}
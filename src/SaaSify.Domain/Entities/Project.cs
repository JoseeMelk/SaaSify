using SaaSify.Domain.Common;
using SaaSify.Domain.Enums;
using SaaSify.Domain.Exceptions;

namespace SaaSify.Domain.Entities;

public class Project : Entity
{
    public Guid OwnerId { get; private set; }
    public string Name { get; private set; } = null!;
    public string Slug { get; private set; } = null!;

    // Solo se guarda el hash — el key real se muestra una sola vez al crear
    public string ApiKeyHash { get; private set; } = null!;

    // Prefijo visible para lookup rápido (ej: "sk_live_ab12")
    public string ApiKeyPrefix { get; private set; } = null!;

    public ProjectStatus Status { get; private set; }

    private readonly List<Plan> _plans = [];
    public IReadOnlyCollection<Plan> Plans => _plans.AsReadOnly();

    private readonly List<Customer> _customers = [];
    public IReadOnlyCollection<Customer> Customers => _customers.AsReadOnly();

    // Constructor para EF Core
    private Project() { }

    public static Project Create(Guid ownerId, string name, string slug, string apiKeyHash, string apiKeyPrefix)
    {
        if (ownerId == Guid.Empty)
            throw new DomainException("OwnerId is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Project name is required.");

        if (string.IsNullOrWhiteSpace(slug))
            throw new DomainException("Project slug is required.");

        return new Project
        {
            OwnerId = ownerId,
            Name = name.Trim(),
            Slug = slug.Trim().ToLowerInvariant(),
            ApiKeyHash = apiKeyHash,
            ApiKeyPrefix = apiKeyPrefix,
            Status = ProjectStatus.Active
        };
    }

    public void Suspend() => Status = ProjectStatus.Suspended;

    public void Activate() => Status = ProjectStatus.Active;

    public void UpdateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Project name is required.");

        Name = name.Trim();
        MarkAsUpdated();
    }

    public void RotateApiKey(string newApiKeyHash, string newApiKeyPrefix)
    {
        ApiKeyHash = newApiKeyHash;
        ApiKeyPrefix = newApiKeyPrefix;
        MarkAsUpdated();
    }
}
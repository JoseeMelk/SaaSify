using SaaSify.Domain.Common;
using SaaSify.Domain.Exceptions;

namespace SaaSify.Domain.Entities;

public class Customer : Entity
{
    public Guid ProjectId { get; private set; }

    // ID del usuario en el SaaS del dev — SaaSify no sabe nada más de él
    public string ExternalId { get; private set; } = null!;

    // Opcionales — solo para referencia del dev en el dashboard
    public string? Email { get; private set; }
    public string? Name { get; private set; }

    private readonly List<Subscription> _subscriptions = [];
    public IReadOnlyCollection<Subscription> Subscriptions => _subscriptions.AsReadOnly();

    // Constructor para EF Core
    private Customer() { }

    public static Customer Create(Guid projectId, string externalId, string? email = null, string? name = null)
    {
        if (projectId == Guid.Empty)
            throw new DomainException("ProjectId is required.");

        if (string.IsNullOrWhiteSpace(externalId))
            throw new DomainException("ExternalId is required.");

        return new Customer
        {
            ProjectId = projectId,
            ExternalId = externalId.Trim(),
            Email = email?.Trim().ToLowerInvariant(),
            Name = name?.Trim()
        };
    }

    public Subscription? ActiveSubscription =>
        _subscriptions.FirstOrDefault(s => s.Status == Enums.SubscriptionStatus.Active);

    public bool HasActiveSubscription => ActiveSubscription is not null;

    public void UpdateInfo(string? email, string? name)
    {
        Email = email?.Trim().ToLowerInvariant();
        Name = name?.Trim();
        MarkAsUpdated();
    }
}
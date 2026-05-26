using SaaSify.Domain.Common;
using SaaSify.Domain.Enums;
using SaaSify.Domain.Exceptions;

namespace SaaSify.Domain.Entities;

public class Plan : Entity
{
    public Guid ProjectId { get; private set; }
    public string Name { get; private set; } = null!;

    // Identificador público que el dev usa en su código (ej: "pro", "enterprise")
    public string Slug { get; private set; } = null!;

    public decimal? Price { get; private set; }
    public string? Currency { get; private set; }
    public BillingCycle? BillingCycle { get; private set; }

    public bool IsActive { get; private set; }
    public bool IsPublic { get; private set; }

    private readonly List<Feature> _features = [];
    public IReadOnlyCollection<Feature> Features => _features.AsReadOnly();

    private readonly List<Subscription> _subscriptions = [];
    public IReadOnlyCollection<Subscription> Subscriptions => _subscriptions.AsReadOnly();

    // Constructor para EF Core
    private Plan() { }

    public static Plan Create(
        Guid projectId,
        string name,
        string slug,
        decimal? price = null,
        string? currency = null,
        BillingCycle? billingCycle = null,
        bool isPublic = true)
    {
        if (projectId == Guid.Empty)
            throw new DomainException("ProjectId is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainException("Plan name is required.");

        if (string.IsNullOrWhiteSpace(slug))
            throw new DomainException("Plan slug is required.");

        if (price.HasValue && price < 0)
            throw new DomainException("Price cannot be negative.");

        if (price.HasValue && price > 0 && string.IsNullOrWhiteSpace(currency))
            throw new DomainException("Currency is required when price is greater than zero.");

        return new Plan
        {
            ProjectId = projectId,
            Name = name.Trim(),
            Slug = slug.Trim().ToLowerInvariant(),
            Price = price,
            Currency = currency?.Trim().ToUpperInvariant(),
            BillingCycle = billingCycle,
            IsActive = true,
            IsPublic = isPublic
        };
    }

    public void AddFeature(string featureSlug, bool isEnabled = true)
    {
        if (string.IsNullOrWhiteSpace(featureSlug))
            throw new DomainException("Feature slug is required.");

        if (_features.Any(f => f.Slug == featureSlug.ToLowerInvariant()))
            throw new BusinessRuleException($"Feature '{featureSlug}' already exists in this plan.");

        _features.Add(Feature.Create(Id, featureSlug, isEnabled));
        MarkAsUpdated();
    }

    public void UpdateFeature(string featureSlug, bool isEnabled)
    {
        var feature = _features.FirstOrDefault(f => f.Slug == featureSlug.ToLowerInvariant())
            ?? throw new NotFoundException(nameof(Feature), featureSlug);

        feature.SetEnabled(isEnabled);
        MarkAsUpdated();
    }

    public bool HasFeature(string featureSlug) =>
        _features.Any(f => f.Slug == featureSlug.ToLowerInvariant() && f.IsEnabled);

    // Un plan con suscripciones activas no puede eliminarse — solo desactivarse
    public void Deactivate()
    {
        if (_subscriptions.Any(s => s.Status == SubscriptionStatus.Active))
            throw new BusinessRuleException("Cannot deactivate a plan with active subscriptions.");

        IsActive = false;
        MarkAsUpdated();
    }

    public void Activate()
    {
        IsActive = true;
        MarkAsUpdated();
    }
}
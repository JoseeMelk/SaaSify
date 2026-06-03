using SaaSify.Domain.Common;
using SaaSify.Domain.Exceptions;

namespace SaaSify.Domain.Entities;

public class Feature : Entity
{
    public Guid PlanId { get; private set; }

    // El dev define el slug libremente: "export_csv", "advanced_analytics"
    public string Slug { get; private set; } = null!;
    public bool IsEnabled { get; private set; }

    // Constructor para EF Core
    private Feature() { }

    internal static Feature Create(Guid planId, string slug, bool isEnabled = true)
    {
        if (planId == Guid.Empty)
            throw new DomainException("PlanId is required.");

        if (string.IsNullOrWhiteSpace(slug))
            throw new DomainException("Feature slug is required.");

        return new Feature
        {
            PlanId = planId,
            Slug = slug.Trim().ToLowerInvariant(),
            IsEnabled = isEnabled
        };
    }

    internal void SetEnabled(bool isEnabled)
    {
        IsEnabled = isEnabled;
        MarkAsUpdated();
    }
}
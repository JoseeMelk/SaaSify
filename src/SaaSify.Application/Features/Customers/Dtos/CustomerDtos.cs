namespace SaaSify.Application.Features.Customers.Dtos;

public class CreateCustomerRequest
{
    public string ExternalId { get; set; } = null!;
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string PlanSlug { get; set; } = null!;
}

public class CustomerResponse
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string ExternalId { get; set; } = null!;
    public string? Name { get; set; }
    public string? Email { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public SubscriptionSummary? ActiveSubscription { get; set; }
}

public class SubscriptionSummary
{
    public string PlanName { get; set; } = null!;
    public string PlanSlug { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime CurrentPeriodEnd { get; set; }
    public string BillingCycle { get; set; } = null!;
}

// En el list — solo datos básicos
public class CustomerListResponse  // DTO separado para la lista
{
    public Guid Id { get; set; }
    public string ExternalId { get; set; } = null!;
    public string? Name { get; set; }
    public string? Email { get; set; }
    public DateTime CreatedAt { get; set; }
    // Sin ActiveSubscription — eso es para el detalle
}
namespace SaaSify.Application.Features.Subscriptions.Dtos;

public class AssignPlanRequest
{
    public string PlanSlug { get; set; } = null!;
    public string BillingCycle { get; set; } = null!;
    public string? PaymentMethod { get; set; }
    public string? ExternalPaymentRef { get; set; }
}

public class RenewSubscriptionRequest
{
    public string? ExternalPaymentRef { get; set; }
}

public class CancelSubscriptionRequest
{
    public bool Immediately { get; set; }  // true = cancelar ahora, false = cancelar al final del periodo
}

public class SubscriptionResponse
{
    public Guid Id { get; set; }
    public Guid CustomerId { get; set; }
    public Guid PlanId { get; set; }
    public string PlanName { get; set; } = null!;
    public string PlanSlug { get; set; } = null!;
    public string Status { get; set; } = null!;
    public string BillingCycle { get; set; } = null!;
    public string? PaymentMethod { get; set; }
    public string? ExternalPaymentRef { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime CurrentPeriodStart { get; set; }
    public DateTime CurrentPeriodEnd { get; set; }
    public DateTime? RenewsAt { get; set; }
    public bool CancelAtPeriodEnd { get; set; }
    public DateTime? CanceledAt { get; set; }
}


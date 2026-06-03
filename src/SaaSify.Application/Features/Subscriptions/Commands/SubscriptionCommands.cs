using MediatR;
using SaaSify.Application.Common;
using SaaSify.Application.Features.Subscriptions.Dtos;

namespace SaaSify.Application.Features.Subscriptions.Commands;

public class AssignPlanCommand : Command<Result<SubscriptionResponse>>
{
    public Guid OwnerId { get; set; } // Del JWT
    public Guid ProjectId { get; set; } // De la URL
    public string ExternalId { get; set; } = null!; // de la URL — el ID del customer en el sistema del developer
    public string PlanSlug { get; set; } = null!;
    public string BillingCycle { get; set; } = null!;
    public string? PaymentMethod { get; set; }
    public string? ExternalPaymentRef { get; set; }
}

public class RenewSubscriptionCommand : Command<Result<SubscriptionResponse>>
{
    public Guid OwnerId { get; set; } // Del JWT
    public Guid ProjectId { get; set; } // De la URL
    public string ExternalId { get; set; } = null!; // de la URL
    public string? ExternalPaymentRef { get; set; }
}

public class CancelSubscriptionCommand : Command<Result<SubscriptionResponse>>
{
    public Guid OwnerId { get; set; } // Del JWT
    public Guid ProjectId { get; set; } // De la URL
    public string ExternalId { get; set; } = null!; // de la URL
    public bool Immediately { get; set; }
}
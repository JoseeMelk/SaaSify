using SaaSify.Application.Common;
using SaaSify.Application.Features.Subscriptions.Dtos;

namespace SaaSify.Application.Features.Subscriptions.Queries;

public class GetActiveSubscriptionQuery : Query<Result<SubscriptionResponse>>
{
    public Guid OwnerId { get; set; }    // del JWT
    public Guid ProjectId { get; set; } // de la URL
    public string ExternalId { get; set; } = null!; // de la URL
}
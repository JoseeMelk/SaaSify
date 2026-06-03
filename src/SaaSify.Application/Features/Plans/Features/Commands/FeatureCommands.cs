using SaaSify.Application.Common;
using SaaSify.Application.Features.Plans.Features.Dtos;

namespace SaaSify.Application.Features.Plans.Features.Commands;

public class CreateFeatureCommand : Command<Result<FeatureResponse>>
{
    public Guid OwnerId { get; set; }    // del JWT
    public Guid ProjectId { get; set; } // de la URL
    public Guid PlanId { get; set; }    // de la URL
    public string Slug { get; set; } = null!;
    public bool IsEnabled { get; set; }
}

public class RemoveFeatureCommand : Command<Result>
{
    public Guid OwnerId { get; set; }    // del JWT
    public Guid ProjectId { get; set; } // de la URL
    public Guid PlanId { get; set; }    // de la URL
    public Guid FeatureId { get; set; } // de la URL
}
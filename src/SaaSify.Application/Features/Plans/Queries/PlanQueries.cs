using SaaSify.Application.Common;
using SaaSify.Application.Features.Plans.Dtos;

namespace SaaSify.Application.Features.Plans.Queries;

public class GetPlanBySlugQuery : Query<Result<PlanResponse>>
{
    public Guid OwnerId { get; set; } // Viene de JWT

    public Guid ProjectId { get; set; } // viene de la URL
    public string Slug { get; set; } = null!;
}

public class ListPlansQuery : Query<Result<IReadOnlyList<PlanResponse>>>
{
    public Guid OwnerId { get; set; } // Viene de JWT
    public Guid ProjectId { get; set; } // viene de la URL
}
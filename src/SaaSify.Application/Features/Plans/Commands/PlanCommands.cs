using SaaSify.Application.Common;
using SaaSify.Application.Features.Plans.Dtos;

namespace SaaSify.Application.Features.Plans.Commands;

public class CreatePlanCommand : Command<Result<PlanResponse>>
{
    public Guid OwnerId { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = null!;
    public string? Slug { get; set; }
    public decimal? Price { get; set; }
    public string? Currency { get; set; }
    public string? BillingCycle { get; set; }
    public bool IsPublic { get; set; }
}

public class DeactivatePlanCommand : Command<Result>
{
    public Guid Id { get; set; }
    public Guid OwnerId { get; set; }
    public Guid ProjectId { get; set; }
}
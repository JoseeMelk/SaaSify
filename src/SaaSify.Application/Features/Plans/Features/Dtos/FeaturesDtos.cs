namespace SaaSify.Application.Features.Plans.Features.Dtos;

public class CreateFeatureRequest
{
    public string Slug { get; set; } = null!;
    public bool IsEnabled { get; set; } = true; // por defecto habilitada
}

public class FeatureResponse
{
    public Guid Id { get; set; }
    public Guid PlanId { get; set; }
    public string Slug { get; set; } = null!;
    public bool IsEnabled { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
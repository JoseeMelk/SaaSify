namespace SaaSify.Application.Features.Plans.Dtos;

public class CreatePlanRequest
{
    public string Name { get; set; } = null!;
    public string? Slug { get; set; }
    public decimal? Price { get; set; }
    public string? Currency { get; set; }
    public string? BillingCycle { get; set; }
    public bool IsPublic { get; set; }
}


// public class UpdatePlanRequest
// {
//     public string Name { get; set; } = null!;
//     public string Price { get; set; } = null!;
//     public string Currency { get; set; } = null!;
//     public string BillingCycle { get; set; } = null!;
//     public bool IsActive { get; set; }
//     public bool IsPublic { get; set; }
// }

public class DeactivatePlanRequest
{
    
}

public class FeatureResponse
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = null!;
    public bool IsEnabled { get; set; }
}

public class PlanResponse
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public decimal? Price { get; set; }
    public string? Currency { get; set; }
    public string? BillingCycle { get; set; }
    public bool IsActive { get; set; }
    public bool IsPublic { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public IReadOnlyList<FeatureResponse> Features { get; set; } = [];
}
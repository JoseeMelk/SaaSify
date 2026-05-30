using SaaSify.Application.Common;
using SaaSify.Application.Features.Customers.Dtos;

namespace SaaSify.Application.Features.Customers.Commands;

public class CreateCustomerCommand : Command<Result<CustomerResponse>>
{
    public Guid OwnerId { get; set; } // Del JWT
    public Guid ProjectId { get; set; }  // Del URL
    public string ExternalId { get; set; } = null!;
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string PlanSlug { get; set; } = null!; // Plan asignado inicalmente
}
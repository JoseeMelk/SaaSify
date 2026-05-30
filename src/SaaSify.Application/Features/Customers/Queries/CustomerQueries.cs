using MediatR;
using SaaSify.Application.Common;
using SaaSify.Application.Features.Customers.Dtos;

namespace SaaSify.Application.Features.Customers.Queries;

public class GetCustomerByExternalIdQuery : Query<Result<CustomerResponse>>
{
    public Guid OwnerId { get; set; } // Del JWT
    public Guid ProjectId { get; set; } // De la URL
    public string ExternalId { get; set; } = null!; // De la URL 
}

public class ListCustomersQuery : Query<Result<IReadOnlyList<CustomerListResponse>>>
{
    public Guid OwnerId { get; set; } // Del JWT
    public Guid ProjectId { get; set; } // De la URL
}
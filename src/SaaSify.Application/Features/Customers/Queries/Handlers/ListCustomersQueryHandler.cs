using MediatR;
using SaaSify.Application.Common;
using SaaSify.Application.Features.Customers.Dtos;
using SaaSify.Application.Features.Customers.Queries;
using SaaSify.Domain.Interfaces;

namespace SaaSify.Application.Features.Customers.Queries.Handlers;

public class ListCustomersQueryHandler : IRequestHandler<ListCustomersQuery, Result<IReadOnlyList<CustomerListResponse>>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IProjectRepository _projectRepository;

    public ListCustomersQueryHandler(
        ICustomerRepository customerRepository,
        IProjectRepository projectRepository
    )
    {
        _customerRepository = customerRepository;
        _projectRepository = projectRepository;
    }

    public async Task<Result<IReadOnlyList<CustomerListResponse>>> Handle(
        ListCustomersQuery request, CancellationToken cancellationToken
    )
    {
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project == null) return Result<IReadOnlyList<CustomerListResponse>>.Failure("Project not found", errorCode: 404);

        if (project.OwnerId != request.OwnerId) return Result<IReadOnlyList<CustomerListResponse>>.Failure("Unauthorized", errorCode: 401);

        var customers = await _customerRepository.GetByProjectIdAsync(project.Id, cancellationToken);
        var customerResponses = customers.Select(customer => new CustomerListResponse
        {
            Id = customer.Id,
            ExternalId = customer.ExternalId,
            Name = customer.Name,
            Email = customer.Email,
            CreatedAt = customer.CreatedAt,
        }).ToList();

        return Result<IReadOnlyList<CustomerListResponse>>.Success(customerResponses);
    }
}
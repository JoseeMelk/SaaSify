using MediatR;
using SaaSify.Application.Common;
using SaaSify.Application.Features.Customers.Dtos;
using SaaSify.Application.Features.Customers.Queries;
using SaaSify.Domain.Interfaces;

namespace SaaSify.Application.Features.Customers.Queries.Handlers;

public class GetCustomerByExternalIdQueryHandler : IRequestHandler<GetCustomerByExternalIdQuery, Result<CustomerResponse>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IPlanRepository _planRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;

    public GetCustomerByExternalIdQueryHandler(ICustomerRepository customerRepository, IProjectRepository projectRepository, IPlanRepository planRepository, ISubscriptionRepository subscriptionRepository)
    {
        _customerRepository = customerRepository;
        _projectRepository = projectRepository;
        _planRepository = planRepository;
        _subscriptionRepository = subscriptionRepository;
    }

    public async Task<Result<CustomerResponse>> Handle(
        GetCustomerByExternalIdQuery request, CancellationToken cancellationToken
    )
    {
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (project == null) return Result<CustomerResponse>.Failure("Project not found", errorCode: 404);

        if (project.OwnerId != request.OwnerId) return Result<CustomerResponse>.Failure("Unauthorized", errorCode: 401);

        var customer = await _customerRepository.GetByExternalIdAsync(request.ProjectId, request.ExternalId, cancellationToken);
        if (customer == null) return Result<CustomerResponse>.Failure("Customer not found", errorCode: 404);

        var customerSubscription = await _subscriptionRepository.GetActiveByCustomerIdAsync(customer.Id, cancellationToken);

        SubscriptionSummary? subscriptionSummary = null;

        if (customerSubscription is not null)
        {
            var plan = await _planRepository.GetByIdAsync(
                customerSubscription.PlanId,
                cancellationToken);

            if (plan is not null)
            {
                subscriptionSummary = new SubscriptionSummary
                {
                    PlanName = plan.Name,
                    PlanSlug = plan.Slug,
                    Status = customerSubscription.Status.ToString(),
                    CurrentPeriodEnd = customerSubscription.CurrentPeriodEnd,
                    BillingCycle = customerSubscription.BillingCycle.ToString()
                };
            }
        }

        return Result<CustomerResponse>.Success(
            new CustomerResponse
            {
                Id = customer.Id,
                ProjectId = customer.ProjectId,
                ExternalId = customer.ExternalId,
                Name = customer.Name,
                Email = customer.Email,
                CreatedAt = customer.CreatedAt,
                UpdatedAt = customer.UpdatedAt,
                ActiveSubscription = subscriptionSummary
            });
    }
}
using MediatR;
using SaaSify.Application.Common;
using SaaSify.Application.Features.Customers.Commands;
using SaaSify.Application.Features.Customers.Dtos;
using SaaSify.Domain.Interfaces;
using SaaSify.Domain.Entities;
using SaaSify.Domain.Enums;

namespace SaaSify.Application.Features.Customers.Commands.Handlers;

public class CreateCustomerCommandHandler : IRequestHandler<CreateCustomerCommand, Result<CustomerResponse>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IPlanRepository _planRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateCustomerCommandHandler(
        ICustomerRepository customerRepository,
        IProjectRepository projectRepository,
        IPlanRepository planRepository,
        ISubscriptionRepository subscriptionRepository,
        IUnitOfWork unitOfWork)
    {
        _customerRepository = customerRepository;
        _projectRepository = projectRepository;
        _planRepository = planRepository;
        _subscriptionRepository = subscriptionRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CustomerResponse>> Handle(CreateCustomerCommand request, CancellationToken cancellationToken)
    {
        var projectExist = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);
        if (projectExist == null) return Result<CustomerResponse>.Failure("Project not found", errorCode: 404);

        if (projectExist.OwnerId != request.OwnerId) return Result<CustomerResponse>.Failure("Unauthorized", errorCode: 403);

        var externalIdExist = await _customerRepository.ExistsByExternalIdAsync(request.ProjectId, request.ExternalId, cancellationToken);
        if (externalIdExist) return Result<CustomerResponse>.Failure("Customer already exists", errorCode: 409);

        var planExist = await _planRepository.GetBySlugAsync(request.ProjectId, request.PlanSlug, cancellationToken);
        if (planExist == null || !planExist.IsActive) return Result<CustomerResponse>.Failure("Plan not found", errorCode: 404);

        var customer = Customer.Create(
            projectId: request.ProjectId,
            externalId: request.ExternalId,
            name: request.Name,
            email: request.Email
        );

        var billingCycle = planExist.BillingCycle ?? BillingCycle.Monthly;

        var subscription = Subscription.Create(
            customerId: customer.Id,
            planId: planExist.Id,
            billingCycle: billingCycle
        );

        await _customerRepository.AddAsync(customer, cancellationToken);
        await _subscriptionRepository.AddAsync(subscription, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<CustomerResponse>.Success(new CustomerResponse
        {
            Id = customer.Id,
            ProjectId = customer.ProjectId,
            ExternalId = customer.ExternalId,
            Name = customer.Name,
            Email = customer.Email,
            CreatedAt = customer.CreatedAt,
            UpdatedAt = customer.UpdatedAt,
            ActiveSubscription = new SubscriptionSummary
            {
                PlanName = planExist.Name,
                PlanSlug = planExist.Slug,
                Status = subscription.Status.ToString(),
                CurrentPeriodEnd = subscription.CurrentPeriodEnd,
                BillingCycle = subscription.BillingCycle.ToString()
            }
        });
    }
}
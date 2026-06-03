using MediatR;
using SaaSify.Application.Common;
using SaaSify.Application.Features.Subscriptions.Commands;
using SaaSify.Application.Features.Subscriptions.Dtos;
using SaaSify.Domain.Entities;
using SaaSify.Domain.Enums;
using SaaSify.Domain.Interfaces;

namespace SaaSify.Application.Features.Subscriptions.Commands.Handlers;

public class CancelSubscriptionCommandHandler : IRequestHandler<CancelSubscriptionCommand, Result<SubscriptionResponse>>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IPlanRepository _planRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CancelSubscriptionCommandHandler(
        ISubscriptionRepository subscriptionRepository,
        ICustomerRepository customerRepository,
        IProjectRepository projectRepository,
        IPlanRepository planRepository,
        IUnitOfWork unitOfWork)
    {
        _subscriptionRepository = subscriptionRepository;
        _customerRepository = customerRepository;
        _projectRepository = projectRepository;
        _planRepository = planRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<SubscriptionResponse>> Handle(
        CancelSubscriptionCommand request,
        CancellationToken cancellationToken)
    {
        // ── Paso 1: Verificar proyecto y owner ────────────────────────────
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);

        if (project == null)
            return Result<SubscriptionResponse>.Failure("Project not found", errorCode: 404);

        if (project.OwnerId != request.OwnerId)
            return Result<SubscriptionResponse>.Failure("Unauthorized", errorCode: 403);

        // ── Paso 2: Buscar el customer ────────────────────────────────────
        var customer = await _customerRepository.GetByExternalIdAsync(
            request.ProjectId,
            request.ExternalId,
            cancellationToken);

        if (customer == null)
            return Result<SubscriptionResponse>.Failure("Customer not found", errorCode: 404);

        // ── Paso 3: Obtener suscripción activa ────────────────────────────
        var subscription = await _subscriptionRepository
            .GetActiveByCustomerIdAsync(customer.Id, cancellationToken);

        if (subscription == null)
            return Result<SubscriptionResponse>.Failure(
                "No active subscription found",
                errorCode: 404);

        // ── Paso 4: Buscar el plan para el mapeo ──────────────────────────
        var plan = await _planRepository.GetByIdAsync(subscription.PlanId, cancellationToken);

        if (plan == null)
            return Result<SubscriptionResponse>.Failure("Plan not found", errorCode: 404);

        // ── Paso 5: Cancelar según el tipo ───────────────────────────────
        // Immediately = true → el customer pierde acceso ahora mismo
        //   Status = Cancelled, RenewsAt = null
        //
        // Immediately = false → el customer mantiene acceso hasta CurrentPeriodEnd
        //   CancelAtPeriodEnd = true, RenewsAt = null
        //   El background job lo marcará Expired cuando venza
        if (request.Immediately)
            subscription.CancelImmediately();
        else
            subscription.CancelAtEnd();

        _subscriptionRepository.Update(subscription);

        // ── Paso 6: Guardar ───────────────────────────────────────────────
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result<SubscriptionResponse>.Success(MapToResponse(subscription, plan));
    }

    private static SubscriptionResponse MapToResponse(Subscription subscription, Plan plan) =>
        new()
        {
            Id = subscription.Id,
            CustomerId = subscription.CustomerId,
            PlanId = subscription.PlanId,
            PlanName = plan.Name,
            PlanSlug = plan.Slug,
            Status = subscription.Status.ToString(),
            BillingCycle = subscription.BillingCycle.ToString(),
            PaymentMethod = subscription.PaymentMethod?.ToString(),
            ExternalPaymentRef = subscription.ExternalPaymentRef,
            StartedAt = subscription.StartedAt,
            CurrentPeriodStart = subscription.CurrentPeriodStart,
            CurrentPeriodEnd = subscription.CurrentPeriodEnd,
            RenewsAt = subscription.RenewsAt,
            CancelAtPeriodEnd = subscription.CancelAtPeriodEnd,
            CanceledAt = subscription.CancelledAt
        };
}
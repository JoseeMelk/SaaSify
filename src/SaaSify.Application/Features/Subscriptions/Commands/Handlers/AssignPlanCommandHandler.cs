using MediatR;
using SaaSify.Application.Common;
using SaaSify.Application.Features.Subscriptions.Commands;
using SaaSify.Application.Features.Subscriptions.Dtos;
using SaaSify.Domain.Entities;
using SaaSify.Domain.Enums;
using SaaSify.Domain.Interfaces;

namespace SaaSify.Application.Features.Subscriptions.Commands.Handlers;

public class AssignPlanCommandHandler : IRequestHandler<AssignPlanCommand, Result<SubscriptionResponse>>
{
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly ICustomerRepository _customerRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IPlanRepository _planRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AssignPlanCommandHandler(
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
        AssignPlanCommand request,
        CancellationToken cancellationToken)
    {
        // ── Paso 1: Verificar que el proyecto existe y pertenece al owner ──
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);

        if (project == null)
            return Result<SubscriptionResponse>.Failure("Project not found", errorCode: 404);

        if (project.OwnerId != request.OwnerId)
            return Result<SubscriptionResponse>.Failure("Unauthorized", errorCode: 403);

        // ── Paso 2: Buscar el customer por ExternalId ──────────────────────
        var customer = await _customerRepository.GetByExternalIdAsync(
            request.ProjectId,
            request.ExternalId,
            cancellationToken);

        if (customer == null)
            return Result<SubscriptionResponse>.Failure("Customer not found", errorCode: 404);

        // ── Paso 3: Buscar el plan por slug — debe existir y estar activo ──
        var plan = await _planRepository.GetBySlugAsync(
            request.ProjectId,
            request.PlanSlug,
            cancellationToken);

        if (plan == null || !plan.IsActive)
            return Result<SubscriptionResponse>.Failure("Plan not found or inactive", errorCode: 404);

        // ── Paso 4: Parsear BillingCycle de string a enum ──────────────────
        if (!Enum.TryParse<BillingCycle>(request.BillingCycle, ignoreCase: true, out var billingCycle))
            return Result<SubscriptionResponse>.Failure(
                $"Invalid BillingCycle. Allowed: {string.Join(", ", Enum.GetNames<BillingCycle>())}",
                errorCode: 400);

        // ── Paso 5: Parsear PaymentMethod si viene ─────────────────────────
        PaymentMethod? paymentMethod = null;

        if (!string.IsNullOrWhiteSpace(request.PaymentMethod))
        {
            if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, ignoreCase: true, out var parsedMethod))
                return Result<SubscriptionResponse>.Failure(
                    $"Invalid PaymentMethod. Allowed: {string.Join(", ", Enum.GetNames<PaymentMethod>())}",
                    errorCode: 400);

            paymentMethod = parsedMethod;
        }

        // ── Paso 6: Cancelar suscripción activa si existe ──────────────────
        // Cambiar de plan = cancelar la actual + crear una nueva.
        // El historial queda limpio — nunca se modifica una suscripción existente.
        var activeSubscription = await _subscriptionRepository
            .GetActiveByCustomerIdAsync(customer.Id, cancellationToken);

        if (activeSubscription != null)
        {
            activeSubscription.CancelImmediately();
            _subscriptionRepository.Update(activeSubscription);
        }

        // ── Paso 7: Crear nueva suscripción ───────────────────────────────
        var subscription = Subscription.Create(
            customerId: customer.Id,
            planId: plan.Id,
            billingCycle: billingCycle,
            paymentMethod: paymentMethod,
            externalPaymentRef: request.ExternalPaymentRef);

        await _subscriptionRepository.AddAsync(subscription, cancellationToken);

        // ── Paso 8: Guardar todo en una sola transacción ───────────────────
        // La cancelación y la nueva suscripción se guardan juntas.
        // Si algo falla, ninguna queda a medias.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ── Paso 9: Devolver SubscriptionResponse ─────────────────────────
        return Result<SubscriptionResponse>.Success(MapToResponse(subscription, plan));
    }

    /// <summary>
    /// Mapea Subscription + Plan a SubscriptionResponse.
    /// Se necesita el plan separado porque Subscription no carga
    /// la navegación a Plan automáticamente después de crear.
    /// </summary>
    private static SubscriptionResponse MapToResponse(Subscription subscription, Plan plan)
    {
        return new SubscriptionResponse
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
}
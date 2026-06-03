using MediatR;
using SaaSify.Application.Common;
using SaaSify.Application.Features.Entitlements.Dtos;
using SaaSify.Application.Features.Entitlements.Queries;
using SaaSify.Domain.Interfaces;

namespace SaaSify.Application.Features.Entitlements.Queries.Handlers;

public class CheckEntitlementQueryHandler
    : IRequestHandler<CheckEntitlementQuery, Result<EntitlementCheckResponse>>
{
    private readonly ICustomerRepository _customerRepository;
    private readonly ISubscriptionRepository _subscriptionRepository;
    private readonly IPlanRepository _planRepository;

    public CheckEntitlementQueryHandler(
        ICustomerRepository customerRepository,
        ISubscriptionRepository subscriptionRepository,
        IPlanRepository planRepository)
    {
        _customerRepository = customerRepository;
        _subscriptionRepository = subscriptionRepository;
        _planRepository = planRepository;
    }

    public async Task<Result<EntitlementCheckResponse>> Handle(
        CheckEntitlementQuery request,
        CancellationToken cancellationToken)
    {
        // ── Paso 1: Buscar el customer por ExternalId ─────────────────────
        var customer = await _customerRepository.GetByExternalIdAsync(
            request.ProjectId,
            request.ExternalId,
            cancellationToken);

        if (customer == null)
            return Result<EntitlementCheckResponse>.Success(new EntitlementCheckResponse
            {
                Allowed = false,
                Plan = "none",
                Feature = request.Feature,
                Reason = "Customer not found"
            });

        // ── Paso 2: Obtener suscripción activa ────────────────────────────
        var subscription = await _subscriptionRepository
            .GetActiveByCustomerIdAsync(customer.Id, cancellationToken);

        if (subscription == null)
            return Result<EntitlementCheckResponse>.Success(new EntitlementCheckResponse
            {
                Allowed = false,
                Plan = "none",
                Feature = request.Feature,
                Reason = "No active subscription"
            });

        // ── Paso 3: Verificar que el período no haya vencido ──────────────
        if (!subscription.IsActive)
            return Result<EntitlementCheckResponse>.Success(new EntitlementCheckResponse
            {
                Allowed = false,
                Plan = "expired",
                Feature = request.Feature,
                ExpiresAt = subscription.CurrentPeriodEnd,
                Reason = "Subscription period has expired"
            });

        // ── Paso 4: Obtener el plan CON sus features ───────────────────────
        // Se usa GetByIdWithFeaturesAsync — incluye el Include(p => p.Features).
        // Con GetByIdAsync las features llegan vacías y HasFeature() siempre devuelve false.
        var plan = await _planRepository.GetByIdWithFeaturesAsync(
            subscription.PlanId,
            cancellationToken);

        if (plan == null)
            return Result<EntitlementCheckResponse>.Success(new EntitlementCheckResponse
            {
                Allowed = false,
                Plan = "none",
                Feature = request.Feature,
                Reason = "Plan not found"
            });

        // ── Paso 5: Verificar si la feature está habilitada en el plan ────
        var allowed = plan.HasFeature(request.Feature);

        return Result<EntitlementCheckResponse>.Success(new EntitlementCheckResponse
        {
            Allowed = allowed,
            Plan = plan.Slug,
            Feature = request.Feature,
            ExpiresAt = subscription.CurrentPeriodEnd,
            Reason = allowed ? null : $"Feature '{request.Feature}' is not enabled in plan '{plan.Slug}'"
        });
    }
}
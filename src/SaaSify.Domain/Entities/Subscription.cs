using SaaSify.Domain.Common;
using SaaSify.Domain.Enums;
using SaaSify.Domain.Exceptions;

namespace SaaSify.Domain.Entities;

public class Subscription : Entity
{
    public Guid CustomerId { get; private set; }
    public Guid PlanId { get; private set; }
    public SubscriptionStatus Status { get; private set; }
    public BillingCycle BillingCycle { get; private set; }
    public PaymentMethod? PaymentMethod { get; private set; }

    // Referencia al cobro externo (Stripe, MercadoPago, etc.) — SaaSify no lo procesa
    public string? ExternalPaymentRef { get; private set; }

    public DateTime StartedAt { get; private set; }
    public DateTime CurrentPeriodStart { get; private set; }

    // Fuente de verdad: cuándo vence el período actual
    public DateTime CurrentPeriodEnd { get; private set; }

    // Próxima renovación — null si está cancelada
    public DateTime? RenewsAt { get; private set; }

    // Cancela al vencer el período, no de inmediato
    public bool CancelAtPeriodEnd { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    // Constructor para EF Core
    private Subscription() { }

    public static Subscription Create(
        Guid customerId,
        Guid planId,
        BillingCycle billingCycle,
        PaymentMethod? paymentMethod = null,
        string? externalPaymentRef = null)
    {
        if (customerId == Guid.Empty)
            throw new DomainException("CustomerId is required.");

        if (planId == Guid.Empty)
            throw new DomainException("PlanId is required.");

        var now = DateTime.UtcNow;
        var periodEnd = billingCycle == BillingCycle.Monthly
            ? now.AddMonths(1)
            : now.AddYears(1);

        return new Subscription
        {
            CustomerId = customerId,
            PlanId = planId,
            Status = SubscriptionStatus.Active,
            BillingCycle = billingCycle,
            PaymentMethod = paymentMethod,
            ExternalPaymentRef = externalPaymentRef,
            StartedAt = now,
            CurrentPeriodStart = now,
            CurrentPeriodEnd = periodEnd,
            RenewsAt = periodEnd,
            CancelAtPeriodEnd = false
        };
    }

    // El dev notifica que el cobro fue exitoso — SaaSify renueva el período
    public void Renew(string? externalPaymentRef = null)
    {
        if (Status == SubscriptionStatus.Cancelled)
            throw new BusinessRuleException("Cannot renew a cancelled subscription.");

        var now = DateTime.UtcNow;
        var newPeriodEnd = BillingCycle == BillingCycle.Monthly
            ? CurrentPeriodEnd.AddMonths(1)
            : CurrentPeriodEnd.AddYears(1);

        CurrentPeriodStart = now;
        CurrentPeriodEnd = newPeriodEnd;
        RenewsAt = newPeriodEnd;
        Status = SubscriptionStatus.Active;
        ExternalPaymentRef = externalPaymentRef ?? ExternalPaymentRef;
        MarkAsUpdated();
    }

    // Cancela al final del período — el customer sigue activo hasta que venza
    public void CancelAtEnd()
    {
        if (Status != SubscriptionStatus.Active)
            throw new BusinessRuleException("Only active subscriptions can be cancelled.");

        CancelAtPeriodEnd = true;
        RenewsAt = null;
        CancelledAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    // Cancelación inmediata
    public void CancelImmediately()
    {
        if (Status == SubscriptionStatus.Cancelled)
            throw new BusinessRuleException("Subscription is already cancelled.");

        Status = SubscriptionStatus.Cancelled;
        CancelAtPeriodEnd = false;
        RenewsAt = null;
        CancelledAt = DateTime.UtcNow;
        MarkAsUpdated();
    }

    // Llamado por el background job cuando CurrentPeriodEnd < ahora
    public void MarkAsExpired()
    {
        if (Status == SubscriptionStatus.Cancelled)
            return;

        Status = SubscriptionStatus.Expired;
        RenewsAt = null;
        MarkAsUpdated();
    }

    // El período venció pero el dev aún no confirmó el pago
    public void MarkAsPastDue()
    {
        Status = SubscriptionStatus.PastDue;
        MarkAsUpdated();
    }

    public bool IsActive => Status == SubscriptionStatus.Active
                         && CurrentPeriodEnd > DateTime.UtcNow;
}
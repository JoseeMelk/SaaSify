using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSify.Domain.Entities;

namespace SaaSify.Infrastructure.Persistence.Configurations;

public class SubscriptionConfiguration : IEntityTypeConfiguration<Subscription>
{
    public void Configure(EntityTypeBuilder<Subscription> builder)
    {
        builder.ToTable("subscriptions");

        builder.HasKey(s => s.Id);
        builder.Property(s => s.Id).HasColumnName("id");

        builder.Property(s => s.CustomerId).HasColumnName("customer_id").IsRequired();
        builder.Property(s => s.PlanId).HasColumnName("plan_id").IsRequired();
        builder.Property(s => s.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();
        // Payment information
        builder.Property(s => s.BillingCycle).HasColumnName("billing_cycle").HasConversion<string>().HasMaxLength(20).IsRequired();
        builder.Property(s => s.PaymentMethod).HasColumnName("payment_method").HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.ExternalPaymentRef).HasColumnName("external_payment_ref").HasMaxLength(255);
        // Time control
        builder.Property(s => s.StartedAt).HasColumnName("started_at").IsRequired();
        builder.Property(s => s.CurrentPeriodStart).HasColumnName("current_period_start").IsRequired();
        builder.Property(s => s.CurrentPeriodEnd).HasColumnName("current_period_end").IsRequired();
        builder.Property(s => s.RenewsAt).HasColumnName("renews_at");
        builder.Property(s => s.CancelAtPeriodEnd).HasColumnName("cancel_at_period_end").IsRequired();
        builder.Property(s => s.CancelledAt).HasColumnName("cancelled_at");

        // Timestamp
        builder.Property(s => s.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(s => s.UpdatedAt).HasColumnName("updated_at");
        builder.Property(s => s.DeletedAt).HasColumnName("deleted_at");

        // ── Índices ────────────────────────────────────────────────────────────
        builder.HasIndex(s => s.CustomerId).HasDatabaseName("ix_subscriptions_customer_id");
        builder.HasIndex(s => s.PlanId).HasDatabaseName("ix_subscriptions_plan_id");
        // Para el background job de expiración
        builder.HasIndex(s => s.CurrentPeriodEnd).HasDatabaseName("ix_subscriptions_current_period_end");

        // El índice parcial más importante del sistema:
        // garantiza que solo puede existir UNA suscripción Active por customer.
        // EF Core lo soporta con HasFilter para PostgreSQL.
        builder.HasIndex(s => s.CustomerId).IsUnique().HasFilter("status = 'Active'").HasDatabaseName("uq_subscriptions_customer_active");
    }
}

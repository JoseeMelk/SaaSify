using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSify.Domain.Entities;

namespace SaaSify.Infrastructure.Persistence.Configurations;

public class PlanConfiguration : IEntityTypeConfiguration<Plan>
{
    public void Configure(EntityTypeBuilder<Plan> builder)
    {
        builder.ToTable("plans");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");
        builder.Property(p => p.ProjectId).HasColumnName("project_id").IsRequired();

        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(p => p.Slug).HasColumnName("slug").HasMaxLength(100).IsRequired();
        // Price y Currency son opcionales — un plan Free no tiene precio
        builder.Property(p => p.Price).HasColumnName("price").HasColumnType("decimal(10,2)");
        builder.Property(p => p.Currency).HasColumnName("currency").HasMaxLength(3); // ISO 4217: USD, MXN, EUR
        builder.Property(p => p.BillingCycle).HasColumnName("billing_cycle").HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.IsActive).HasColumnName("is_active").IsRequired();
        builder.Property(p => p.IsPublic).HasColumnName("is_public").IsRequired();

        // Propiedades timestamp
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");
        builder.Property(p => p.DeletedAt).HasColumnName("deleted_at");

        // ── Índices ────────────────────────────────────────────────────────────
        // Slug único dentro de cada proyecto — no globalmente
        builder.HasIndex(p => new { p.ProjectId, p.Slug }).IsUnique().HasDatabaseName("uq_plans_project_id_slug");
        builder.HasIndex(p => p.ProjectId).HasDatabaseName("ix_plans_project_id");

        // ── Relaciones ────────────────────────────────────────────────────────
        builder.HasMany(p => p.Features).WithOne().HasForeignKey(f => f.PlanId).OnDelete(DeleteBehavior.Cascade); // si se borra el plan, se borran sus features
        builder.HasMany(p => p.Subscriptions).WithOne().HasForeignKey(s => s.PlanId).OnDelete(DeleteBehavior.Restrict); // no borrar suscripciones en cascada
    }
}
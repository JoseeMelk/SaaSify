using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSify.Domain.Entities;

namespace SaaSify.Infrastructure.Persistence.Configurations;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects");

        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasColumnName("id");

        builder.Property(p => p.OwnerId).HasColumnName("owner_id").IsRequired();
        builder.Property(p => p.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(p => p.Slug).HasColumnName("slug").HasMaxLength(100).IsRequired();
        builder.Property(p => p.ApiKeyHash).HasColumnName("api_key_hash").HasMaxLength(512).IsRequired();

        // El prefijo permite el lookup rápido sin exponer el hash completo
        builder.Property(p => p.ApiKeyPrefix).HasColumnName("api_key_prefix").HasMaxLength(20).IsRequired();
        builder.Property(p => p.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();

        // Propiedades timestamp
        builder.Property(p => p.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(p => p.UpdatedAt).HasColumnName("updated_at");
        builder.Property(p => p.DeletedAt).HasColumnName("deleted_at");

        // ── Índices ────────────────────────────────────────────────────────────
        builder.HasIndex(p => p.Slug).IsUnique().HasDatabaseName("uq_projects_slug");
        builder.HasIndex(p => p.ApiKeyPrefix).IsUnique().HasDatabaseName("uq_projects_api_key_prefix");
        builder.HasIndex(p => p.OwnerId).HasDatabaseName("ix_projects_owner_id");

        // ── Relaciones ────────────────────────────────────────────────────────
        builder.HasMany(p => p.Plans).WithOne().HasForeignKey(pl => pl.ProjectId).OnDelete(DeleteBehavior.Restrict);
        builder.HasMany(p => p.Customers).WithOne().HasForeignKey(c => c.ProjectId).OnDelete(DeleteBehavior.Restrict);
    }
}
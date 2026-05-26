using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSify.Domain.Entities;

namespace SaaSify.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("customers");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).HasColumnName("id");

        builder.Property(c => c.ProjectId).HasColumnName("project_id").IsRequired();
        builder.Property(c => c.ExternalId).HasColumnName("external_id").HasMaxLength(255).IsRequired();
        // Email y Name opcionales — solo referencia para el dashboard
        builder.Property(c => c.Email).HasColumnName("email").HasMaxLength(320);
        builder.Property(c => c.Name).HasColumnName("name").HasMaxLength(100);
        
        // Timestamp
        builder.Property(c => c.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(c => c.UpdatedAt).HasColumnName("updated_at");
        builder.Property(c => c.DeletedAt).HasColumnName("deleted_at");

        // El índice más importante de Customer:
        // externalId único POR proyecto — no globalmente
        // Dos proyectos distintos pueden tener "user_123", pero no el mismo proyecto
        builder.HasIndex(c => new { c.ProjectId, c.ExternalId }).IsUnique().HasDatabaseName("uq_customers_project_id_external_id");
        builder.HasIndex(c => c.ProjectId).HasDatabaseName("ix_customers_project_id");
        builder.HasMany(c => c.Subscriptions).WithOne().HasForeignKey(s => s.CustomerId).OnDelete(DeleteBehavior.Restrict);
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSify.Domain.Entities;

namespace SaaSify.Infrastructure.Persistence.Configurations;

public class FeatureConfiguration : IEntityTypeConfiguration<Feature>
{
    public void Configure(EntityTypeBuilder<Feature> builder)
    {
        builder.ToTable("features");

        builder.HasKey(f => f.Id);
        builder.Property(f => f.Id).HasColumnName("id");

        builder.Property(f => f.PlanId).HasColumnName("plan_id").IsRequired();
        builder.Property(f => f.Slug).HasColumnName("slug").HasMaxLength(100).IsRequired();
        builder.Property(f => f.IsEnabled).HasColumnName("is_enabled").IsRequired();

        // Timestamp
        builder.Property(f => f.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(f => f.UpdatedAt).HasColumnName("updated_at");
        //builder.Property(f => f.DeletedAt).HasColumnName("deleted_at");

        // Slug único dentro de cada plan
        builder.HasIndex(f => new { f.PlanId, f.Slug }).IsUnique().HasDatabaseName("uq_features_plan_id_slug");
    }
}

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaaSify.Domain.Entities;

namespace SaaSify.Infrastructure.Persistence.Configurations;

// IEntityTypeConfiguration<T> es la forma correcta de configurar entidades en EF Core.
// Separar cada configuración en su propio archivo es mucho más limpio que poner todo en OnModelCreating.
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // Nombre de la tabla
        builder.ToTable("users");

        // Configuración de las propiedades
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Id).HasColumnName("id");

        builder.Property(u => u.Email).HasColumnName("email").HasMaxLength(320).IsRequired();
        builder.Property(u => u.PasswordHash).HasColumnName("password_hash").HasMaxLength(512).IsRequired();
        builder.Property(u => u.Name).HasColumnName("name").HasMaxLength(100).IsRequired();
        builder.Property(u => u.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20).IsRequired();

        // Propiedades timestamp
        builder.Property(u => u.CreatedAt).HasColumnName("created_at").IsRequired();
        builder.Property(u => u.UpdatedAt).HasColumnName("updated_at");
        builder.Property(u => u.DeletedAt).HasColumnName("deleted_at");

        // ── Índices ────────────────────────────────────────────────────────────
        // Email único globalmente — evita registros duplicados
        builder.HasIndex(u => u.Email).IsUnique().HasDatabaseName("uq_users_email");

        // ── Relaciones ────────────────────────────────────────────────────────
        builder.HasMany(u => u.Projects)
            .WithOne() // Project no tiene propiedad de navegación hacia User
            .HasForeignKey(p => p.OwnerId)
            .OnDelete(DeleteBehavior.Restrict); // no borrar proyectos en cascada
    }
}

using Microsoft.EntityFrameworkCore;
using SaaSify.Domain.Entities;
using SaaSify.Domain.Interfaces;

namespace SaaSify.Infrastructure.Persistence;

public class AppDbContext : DbContext, IUnitOfWork
{
    // ── DbSets ────────────────────────────────────────────────────────────────
    // Cada DbSet representa una tabla en la base de datos.
    // EF Core los usa para construir las queries y las migraciones.
    public DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Plan> Plans => Set<Plan>();
    public DbSet<Feature> Features => Set<Feature>();
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<Subscription> Subscriptions => Set<Subscription>();

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Aplica automáticamente todas las configuraciones
        // IEntityTypeConfiguration encontradas en este assembly.
        // Así evitamos registrarlas manualmente una por una.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // ── Global Query Filters ──────────────────────────────────────────────
        // Estos filtros se aplican a TODAS las queries de esa entidad automáticamente.
        // EF Core inyecta el WHERE sin que tenga que recordarlo.
        // Soft delete — nunca devuelve registros eliminados
        modelBuilder.Entity<User>().HasQueryFilter(u => u.DeletedAt == null);
        modelBuilder.Entity<Project>().HasQueryFilter(p => p.DeletedAt == null);
        modelBuilder.Entity<Plan>().HasQueryFilter(p => p.DeletedAt == null);
        modelBuilder.Entity<Feature>().HasQueryFilter(f => f.DeletedAt == null);
        modelBuilder.Entity<Customer>().HasQueryFilter(c => c.DeletedAt == null);
        modelBuilder.Entity<Subscription>().HasQueryFilter(s => s.DeletedAt == null);
    }

    // ── IUnitOfWork ───────────────────────────────────────────────────────────
    // AppDbContext ya tiene SaveChangesAsync heredado de DbContext.
    // Al implementar IUnitOfWork simplemente lo expongo a través del contrato
    // que se define en el Domain — sin agregar nueva lógica.
    // El Application layer solo conoce IUnitOfWork, nunca AppDbContext directamente.
    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Antes de guardar, se actualiza UpdatedAt en todas las entidades modificadas.
        // Así no se depende de que cada entidad llame MarkAsUpdated() manualmente.
        // Es una segunda línea de defensa.

        var modifiedEntries = ChangeTracker.Entries<Domain.Common.Entity>().Where(e => e.State == EntityState.Modified);
        
        foreach (var entry in modifiedEntries)
        {
            // Solo se actualiza si la entidad no lo hizo ya desde el dominio
            if (entry.Entity.UpdatedAt == null)
            {
                entry.Property(nameof(Domain.Common.Entity.UpdatedAt)).CurrentValue = DateTime.UtcNow;
            }
        }

        return base.SaveChangesAsync(cancellationToken);
    }
}
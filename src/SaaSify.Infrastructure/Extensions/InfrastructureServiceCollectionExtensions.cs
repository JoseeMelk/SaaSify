using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SaaSify.Domain.Interfaces;
using SaaSify.Infrastructure.Persistence;
using SaaSify.Infrastructure.Repositories;

namespace SaaSify.Infrastructure.Extensions;

/// <summary>
/// Extensión de IServiceCollection para registrar todos los servicios del Infrastructure Layer.
/// 
/// Se llama desde Program.cs:
///   services.AddInfrastructureServices(configuration);
/// 
/// Registra:
///   - DbContext (EF Core + PostgreSQL)
///   - UnitOfWork
///   - Todos los repositorios
///   - Servicios de auth (JWT, Password Hasher)
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Registrar DbContext ────────────────────────────────────────────
        // Se obtiene la connection string del appsettings.json
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString)
        );

        // ── Registrar UnitOfWork ──────────────────────────────────────────
        // AppDbContext implementa IUnitOfWork, así que se registra como tal.
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<AppDbContext>());

        // ── Registrar repositorios ────────────────────────────────────────
        // Scoped: se crea una nueva instancia por request HTTP.
        // Cada request obtiene su propia conexión a la BD.
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();

        // ── Registrar servicios de autenticación ───────────────────────────
        // Scoped: nueva instancia por request.
        services.AddScoped<SaaSify.Application.Interfaces.IJwtTokenService, 
            SaaSify.Infrastructure.Services.JwtTokenService>();
        services.AddScoped<SaaSify.Application.Interfaces.IPasswordHasher, 
            SaaSify.Infrastructure.Services.PasswordHasher>();

        return services;
    }
}
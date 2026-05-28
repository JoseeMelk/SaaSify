using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;
using SaaSify.Application.Interfaces;
using SaaSify.Domain.Interfaces;
using SaaSify.Infrastructure.Persistence;
using SaaSify.Infrastructure.Repositories;
using SaaSify.Infrastructure.Services;

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
///   - Servicios de auth (JWT, Password Hasher, ApiKey)
///   - Autenticación JWT para ASP.NET Core
/// </summary>
public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // ── Registrar DbContext ────────────────────────────────────────────
        var connectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString)
        );

        // ── Registrar UnitOfWork ──────────────────────────────────────────
        // AppDbContext implementa IUnitOfWork — se registra a través de la interfaz.
        services.AddScoped<IUnitOfWork>(provider =>
            provider.GetRequiredService<AppDbContext>());

        // ── Registrar repositorios ────────────────────────────────────────
        // Scoped: nueva instancia por request HTTP.
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IProjectRepository, ProjectRepository>();
        services.AddScoped<IPlanRepository, PlanRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ISubscriptionRepository, SubscriptionRepository>();

        // ── Registrar servicios de autenticación ───────────────────────────
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IPasswordHasher, PasswordHasher>();
        services.AddScoped<IApiKeyService, ApiKeyService>();

        // ── Configurar autenticación JWT en ASP.NET Core ──────────────────
        // Se configura cómo ASP.NET valida los tokens en cada request.
        // Sin esto, [Authorize] no funciona.
        var jwtSecret = configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret no configurado");

        services.AddAuthentication(options =>
        {
            // Se define JWT Bearer como el esquema por defecto.
            // Cuando llega una request con Authorization: Bearer {token},
            // ASP.NET usa este esquema para validar.
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                // Se valida que el token fue firmado con nuestra clave secreta.
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(jwtSecret)),

                // Se valida que el issuer y audience coincidan con los configurados.
                ValidateIssuer = true,
                ValidIssuer = configuration["Jwt:Issuer"],
                ValidateAudience = true,
                ValidAudience = configuration["Jwt:Audience"],

                // Se valida que el token no haya expirado.
                ValidateLifetime = true,

                // Sin tolerancia de tiempo — el token debe ser exactamente válido.
                ClockSkew = TimeSpan.Zero
            };
        });

        return services;
    }
}
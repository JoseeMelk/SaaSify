using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi;
using SaaSify.Api.Middleware;
using SaaSify.Application.Extensions;
using SaaSify.Infrastructure.Extensions;
using SaaSify.Infrastructure.Persistence;
using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

// ── Servicios ──────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        if (document.Components is null)
            document.Components = new OpenApiComponents();

        var scheme = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Ingresa el access token. Ejemplo: Bearer eyJhbGci..."
        };

        document.Components.SecuritySchemes["Bearer"] = scheme;

        var requirement = new OpenApiSecurityRequirement
        {
            { new OpenApiSecuritySchemeReference("Bearer", document), new List<string>() }
        };

        document.Security = new List<OpenApiSecurityRequirement> { requirement };
        return Task.CompletedTask;
    });
});

// ── Infrastructure + Application ──────────────────────────────────────────
builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();

// ── Authorization policies ─────────────────────────────────────────────────
builder.Services.AddSingleton<Microsoft.AspNetCore.Authorization.IAuthorizationHandler,
    SaaSify.Api.Authorization.AccessTokenRequirementHandler>();

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(SaaSify.Api.Authorization.Policies.RequireAccessToken, policy =>
        policy.Requirements.Add(new SaaSify.Api.Authorization.AccessTokenRequirement()));
});

// ── Health Checks ──────────────────────────────────────────────────────────
// Se registran los checks que se ejecutarán al llamar a /health.
// AddNpgSql verifica que PostgreSQL responda — si no, el sistema no funciona.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddHealthChecks()
    .AddNpgSql(
        connectionString!,
        name: "postgresql",          // nombre del check en la respuesta
        failureStatus: HealthStatus.Unhealthy,
        tags: ["database"]);         // etiqueta para filtrar checks por grupo

var app = builder.Build();

// ── Migración automática en desarrollo ─────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

// ── Pipeline ───────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
    app.MapOpenApi();

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

// GlobalExceptionMiddleware debe ir primero — atrapa excepciones de todo lo demás
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<ApiKeyMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// ── Health Check endpoint ──────────────────────────────────────────────────
// Se expone en /health con una respuesta JSON detallada.
// Devuelve 200 si todo está bien, 503 si algo falla.
//
// Respuesta cuando está sano:
// {
//   "status": "Healthy",
//   "checks": [
//     { "name": "postgresql", "status": "Healthy", "duration": "00:00:00.012" }
//   ]
// }
//
// Respuesta cuando algo falla:
// {
//   "status": "Unhealthy",
//   "checks": [
//     { "name": "postgresql", "status": "Unhealthy", "description": "Connection refused" }
//   ]
// }
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        // Se serializa la respuesta como JSON detallado
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            version = "1.0.0",
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.ToString()
            })
        };

        await context.Response.WriteAsync(
            JsonSerializer.Serialize(response, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }));
    }
});

app.Run();
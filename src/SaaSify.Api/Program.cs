using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using SaaSify.Api.Middleware;
using SaaSify.Application.Extensions;
using SaaSify.Infrastructure.Extensions;
using SaaSify.Infrastructure.Persistence;

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

// ApiKeyMiddleware debe ir antes de UseAuthentication.
// Solo intercepta rutas /api/v1/entitlements — el resto pasa directo.
app.UseMiddleware<ApiKeyMiddleware>();

app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
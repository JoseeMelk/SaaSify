using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using SaaSify.Application.Extensions;
using SaaSify.Infrastructure.Extensions;
using SaaSify.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ── Servicios ──────────────────────────────────────────────────────────────
builder.Services.AddControllers();

// OpenAPI Nativo de .NET 10 (Reemplaza a AddSwaggerGen)
// OpenAPI Nativo de .NET 10 (Sintaxis v2.x corregida sin advertencias de nulos)
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        // Forzamos la inicialización directa del contenedor
        if (document.Components is null)
        {
            document.Components = new OpenApiComponents();
        }
        
        // 1. Configuración del esquema de seguridad Bearer
        var scheme = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Add JWT token, example: Bearer {token}"
        };
        
        // Ahora el compilador sabe con certeza absoluta que Components no es nulo
        document.Components.SecuritySchemes["Bearer"] = scheme;
        
        // 2. Aplicación del requisito de seguridad global con la nueva estructura .NET 10
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

var app = builder.Build();

// ── Migración automática en desarrollo ─────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await dbContext.Database.MigrateAsync();
}

// ── Pipeline de OpenAPI ────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    // Expone el JSON nativo de OpenAPI en: /openapi/v1.json (Reemplaza a UseSwagger)
    app.MapOpenApi(); 
}

if (!app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();

using Microsoft.EntityFrameworkCore;
using SaaSify.Application.Extensions;
using SaaSify.Infrastructure.Extensions;
using SaaSify.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// ── Servicios ──────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "SaaSify API", Version = "v1" });
});

// ── Base de datos ──────────────────────────────────────────────────────────
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(connectionString))
    throw new InvalidOperationException("Connection string 'DefaultConnection' not found in appsettings.");

// Se registran los servicios de Infrastructure (DbContext, repositorios).
builder.Services.AddInfrastructureServices(builder.Configuration);

// Se registran los servicios de Application (MediatR, validadores).
builder.Services.AddApplicationServices();

var app = builder.Build();

// ── Migración automática en desarrollo ─────────────────────────────────────
// Se aplican las migraciones pendientes al iniciar la aplicación.
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await dbContext.Database.MigrateAsync();
    }
}

// ── Pipeline ───────────────────────────────────────────────────────────────
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "SaaSify v1"));
}

// HTTPS solo en producción
if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}

app.UseAuthorization();
app.MapControllers();

app.Run();
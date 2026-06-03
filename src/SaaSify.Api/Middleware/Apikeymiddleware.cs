using SaaSify.Application.Interfaces;
using SaaSify.Domain.Interfaces;

namespace SaaSify.Api.Middleware;

/// <summary>
/// Middleware que autentica requests usando API key en el header X-Api-Key.
/// 
/// Solo se activa en rutas que empiezan con /api/v1/entitlements.
/// El resto de rutas usan JWT normalmente.
/// 
/// Flujo:
/// 1. Lee el header X-Api-Key
/// 2. Extrae el prefijo (primeros 16 caracteres)
/// 3. Busca el proyecto por prefijo en la BD
/// 4. Verifica el hash SHA-256 de la key completa
/// 5. Si es válida, inyecta el ProjectId en HttpContext.Items
/// 6. Si no, devuelve 401
/// 
/// ¿Por qué middleware y no [Authorize]?
/// Porque la autenticación por API key es un mecanismo diferente
/// al JWT. No usa ClaimsPrincipal — usa HttpContext.Items para
/// pasar el ProjectId al controller.
/// </summary>
public class ApiKeyMiddleware
{
    private readonly RequestDelegate _next;
    private const string ApiKeyHeader = "X-Api-Key";
    private const string ProjectIdKey = "ProjectId";
    private const int PrefixLength = 16;

    public ApiKeyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, IProjectRepository projectRepository, IApiKeyService apiKeyService)
    {
        // Solo interceptar rutas de entitlements
        if (!context.Request.Path.StartsWithSegments("/api/v1/entitlements"))
        {
            await _next(context);
            return;
        }

        // ── Paso 1: Leer el header X-Api-Key ──────────────────────────────
        if (!context.Request.Headers.TryGetValue(ApiKeyHeader, out var apiKeyValue))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "X-Api-Key header is required" });
            return;
        }

        var apiKey = apiKeyValue.ToString();

        if (string.IsNullOrWhiteSpace(apiKey) || apiKey.Length < PrefixLength)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid API key format" });
            return;
        }

        // ── Paso 2: Extraer el prefijo ─────────────────────────────────────
        // Los primeros 16 caracteres son el prefijo de búsqueda.
        // Ej: "sk_live_a1b2c3d4" → prefijo = "sk_live_a1b2c3d4"
        var prefix = apiKey[..PrefixLength];

        // ── Paso 3: Buscar el proyecto por prefijo ─────────────────────────
        // Es una búsqueda rápida — el prefijo está indexado en la BD.
        var project = await projectRepository.GetByApiKeyPrefixAsync(prefix);

        if (project == null)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid API key" });
            return;
        }

        // ── Paso 4: Verificar el hash ──────────────────────────────────────
        // Se compara el hash SHA-256 de la key recibida con el hash guardado.
        // FixedTimeEquals evita timing attacks.
        var isValid = apiKeyService.Verify(apiKey, project.ApiKeyHash);

        if (!isValid)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsJsonAsync(new { error = "Invalid API key" });
            return;
        }

        // ── Paso 5: Verificar que el proyecto está activo ─────────────────
        if (project.Status != Domain.Enums.ProjectStatus.Active)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsJsonAsync(new { error = "Project is not active" });
            return;
        }

        // ── Paso 6: Inyectar ProjectId en el contexto ─────────────────────
        // El controller lo lee con HttpContext.Items["ProjectId"].
        context.Items[ProjectIdKey] = project.Id;

        await _next(context);
    }
}
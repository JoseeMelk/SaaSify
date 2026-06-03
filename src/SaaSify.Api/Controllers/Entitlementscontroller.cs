using MediatR;
using Microsoft.AspNetCore.Mvc;
using SaaSify.Api.Extensions;
using SaaSify.Application.Features.Entitlements.Queries;

namespace SaaSify.Api.Controllers;

/// <summary>
/// Controller para el entitlement check.
/// 
/// Este controller NO usa [Authorize] con JWT.
/// La autenticación la hace el ApiKeyMiddleware con X-Api-Key.
/// El ProjectId llega a través de HttpContext.Items, no del JWT.
/// 
/// Es el endpoint más crítico del sistema — el developer lo llama
/// en cada request de su backend para verificar acceso.
/// </summary>
[ApiController]
[Route("api/v1/entitlements")]
public class EntitlementsController : ControllerBase
{
    private readonly IMediator _mediator;

    public EntitlementsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// GET /api/v1/entitlements/check?customerId=user_123&feature=export_csv
    /// X-Api-Key: sk_live_abc123...
    /// 
    /// Verifica si un customer puede acceder a una feature.
    /// 
    /// Siempre devuelve 200 — nunca 403 o 404.
    /// La respuesta siempre tiene allowed: true o false.
    /// Los errores de autenticación devuelven 401 (manejado por el middleware).
    /// </summary>
    [HttpGet("check")]
    public async Task<IActionResult> Check(
        [FromQuery(Name = "customerId")] string externalId,
        [FromQuery] string feature,
        CancellationToken cancellationToken)
    {
        // El ProjectId fue inyectado por ApiKeyMiddleware en HttpContext.Items.
        // Si llegamos aquí, el middleware ya validó la API key.
        var projectId = HttpContext.Items["ProjectId"] as Guid?;

        if (projectId == null || projectId == Guid.Empty)
            return Unauthorized(new { error = "Project could not be identified" });

        var query = new CheckEntitlementQuery
        {
            ProjectId = projectId.Value,
            ExternalId = externalId,
            Feature = feature
        };

        var result = await _mediator.Send(query, cancellationToken);

        // Siempre 200 — el campo allowed indica si tiene acceso o no.
        // El developer nunca recibe 403 en este endpoint — solo true o false.
        return result.ToActionResult(this);
    }
}
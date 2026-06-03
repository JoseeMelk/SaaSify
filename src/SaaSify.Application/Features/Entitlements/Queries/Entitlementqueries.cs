using SaaSify.Application.Common;
using SaaSify.Application.Features.Entitlements.Dtos;

namespace SaaSify.Application.Features.Entitlements.Queries;

/// <summary>
/// Query para verificar si un customer puede acceder a una feature.
/// 
/// ProjectId viene del middleware de API key — no del JWT ni de la URL.
/// ExternalId y Feature vienen de los query params de la URL.
/// 
/// Ejemplo:
/// GET /api/v1/entitlements/check?customerId=user_123&feature=export_csv
/// X-Api-Key: sk_live_abc123...
/// </summary>
public class CheckEntitlementQuery : Query<Result<EntitlementCheckResponse>>
{
    // Extraído del middleware de API key — el proyecto autenticado
    public Guid ProjectId { get; set; }

    // El ID del customer en el sistema del developer
    public string ExternalId { get; set; } = null!;

    // La feature que se quiere verificar
    public string Feature { get; set; } = null!;
}
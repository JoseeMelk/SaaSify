namespace SaaSify.Application.Features.Entitlements.Dtos;

/// <summary>
/// Response del entitlement check.
/// 
/// Es la respuesta más importante del sistema — el developer
/// la usa en su backend para decidir si el customer puede
/// acceder a una feature o no.
/// </summary>
public class EntitlementCheckResponse
{
    // La respuesta principal — true o false
    public bool Allowed { get; set; }

    // El slug del plan actual del customer
    public string Plan { get; set; } = null!;

    // La feature que se consultó
    public string Feature { get; set; } = null!;

    // Cuándo vence el período actual — útil para mostrar en la UI
    public DateTime? ExpiresAt { get; set; }

    // Razón por la que fue denegado — null si Allowed = true
    public string? Reason { get; set; }
}
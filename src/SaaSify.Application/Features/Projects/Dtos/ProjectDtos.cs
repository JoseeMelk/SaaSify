namespace SaaSify.Application.Features.Projects.Dtos;

/// <summary>
/// Request para crear un nuevo proyecto.
/// 
/// El slug es opcional — si no se provee, se genera automáticamente
/// desde el nombre (ej: "Mi SaaS App" → "mi-saas-app").
/// </summary>
public class CreateProjectRequest
{
    public string Name { get; set; } = null!;
    public string? Slug { get; set; } // opcional
}

/// <summary>
/// Response después de crear un proyecto.
/// 
/// Es el ÚNICO momento en que se devuelve el ApiKey completo.
/// Después de esto, SaaSify solo guarda el hash y nunca más
/// puede devolver la key original.
/// </summary>
public class CreateProjectResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;

    // Se devuelve solo en la creación — nunca más después
    public string ApiKey { get; set; } = null!;

    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Response para consultas de un proyecto existente.
/// 
/// No incluye ApiKey — esa información es sensible y
/// no se devuelve después del momento de creación.
/// </summary>
public class ProjectResponse
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

/// <summary>
/// Response después de rotar la API key.
/// 
/// Se devuelve la nueva key completa — es el único momento
/// en que el developer puede verla. Si la pierde, debe rotar de nuevo.
/// </summary>
public class RotateApiKeyResponse
{
    // La nueva API key — visible solo en este momento
    public string ApiKey { get; set; } = null!;
    public DateTime RotatedAt { get; set; }
}
namespace SaaSify.Application.Interfaces;

/// <summary>
/// Resultado de generar una API key.
/// 
/// Se usa record porque es inmutable — una vez generada,
/// sus valores no cambian. Es solo un contenedor de datos.
/// </summary>
public record ApiKeyResult(
    string RawKey,   // sk_live_abc123... — se muestra UNA SOLA VEZ al developer
    string Hash,     // SHA-256 del RawKey — se guarda en la base de datos
    string Prefix    // primeros 12 chars del RawKey — se guarda para lookup rápido
);

/// <summary>
/// Contrato para generar y verificar API keys de proyectos.
/// 
/// La implementación concreta se encuentra en Infrastructure.
/// Los Handlers solo conocen esta interfaz — no saben cómo se genera el hash.
/// </summary>
public interface IApiKeyService
{
    /// <summary>
    /// Se genera una nueva API key segura.
    /// Devuelve los tres componentes: raw key, hash y prefijo.
    /// </summary>
    ApiKeyResult Generate();

    /// <summary>
    /// Se verifica que una raw key coincida con el hash guardado.
    /// Se usa cuando el developer envía su key en los headers.
    /// </summary>
    bool Verify(string rawKey, string hash);
}
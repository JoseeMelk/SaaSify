using SaaSify.Application.Interfaces;

namespace SaaSify.Infrastructure.Services;

/// <summary>
/// Implementación de IPasswordHasher usando BCrypt.
/// 
/// BCrypt es el algoritmo recomendado para hashear passwords porque:
/// - Incluye un salt aleatorio automáticamente (evita rainbow tables)
/// - Es lento por diseño (work factor configurable) — dificulta ataques de fuerza bruta
/// - El hash incluye el salt, así que no hay que guardarlo por separado
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    // Work factor: cuántas rondas de hashing se aplican.
    // 12 es el estándar recomendado — balanceo entre seguridad y performance.
    // Subir a 14+ en producción si el hardware lo permite.
    private const int WorkFactor = 12;

    /// <summary>
    /// Se genera un hash BCrypt de la password.
    /// El salt se genera automáticamente y se incluye en el resultado.
    /// </summary>
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password, WorkFactor);
    }

    /// <summary>
    /// Se verifica que la password coincida con el hash guardado.
    /// BCrypt extrae el salt del hash para comparar correctamente.
    /// </summary>
    public bool VerifyPassword(string password, string hash)
    {
        return BCrypt.Net.BCrypt.Verify(password, hash);
    }
}
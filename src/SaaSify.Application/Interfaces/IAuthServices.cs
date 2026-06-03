namespace SaaSify.Application.Interfaces;

/// <summary>
/// Interfaz para generar y validar JWTs.
/// 
/// La implementación concreta se encuentra en Infrastructure.
/// El Application solo define qué se necesita, no cómo se implementa.
/// </summary>
public interface IJwtTokenService
{
    /// <summary>
    /// Se genera un JWT access token con los datos del usuario.
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="email">Email del usuario</param>
    /// <param name="expiresIn">Duración del token en minutos (default: 15)</param>
    string GenerateAccessToken(Guid userId, string email, int expiresIn = 15);

    /// <summary>
    /// Se genera un refresh token (long-lived) para renovar el access token.
    /// </summary>
    /// <param name="userId">ID del usuario</param>
    /// <param name="expiresInDays">Duración en días (default: 30)</param>
    string GenerateRefreshToken(Guid userId, int expiresInDays = 30);

    /// <summary>
    /// Se valida un JWT y se extraen los claims.
    /// </summary>
    /// <returns>Dictionary con los claims, o null si el token es inválido</returns>
    IDictionary<string, string>? ValidateAccessToken(string token);

    /// <summary>
    /// Se obtiene el userId de un JWT válido.
    /// </summary>
    Guid? GetUserIdFromToken(string token);
}

/// <summary>
/// Interfaz para hashear y verificar passwords.
/// 
/// Se usa BCrypt internamente en la implementación Infrastructure.
/// </summary>
public interface IPasswordHasher
{
    /// <summary>
    /// Se genera un hash seguro de la password.
    /// </summary>
    string HashPassword(string password);

    /// <summary>
    /// Se verifica que una password coincida con el hash guardado.
    /// </summary>
    bool VerifyPassword(string password, string hash);
}
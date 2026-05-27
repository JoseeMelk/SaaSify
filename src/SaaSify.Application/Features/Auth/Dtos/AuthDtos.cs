namespace SaaSify.Application.Features.Auth.Dtos;

/// <summary>
/// Request para registrar un nuevo developer.
/// Se envía con email, password y nombre.
/// </summary>
public class RegisterUserRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Name { get; set; } = null!;
}

/// <summary>
/// Request para login.
/// Se envía con email y password.
/// </summary>
public class LoginUserRequest
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

/// <summary>
/// DTO para la request de logout.
/// Se necesita userId y refresh token para revocar.
/// </summary>
public class LogoutUserRequest
{
    public Guid UserId { get; set; }
    public string RefreshToken { get; set; } = null!;
}

/// <summary>
/// Request para refrescar el JWT.
/// Se envía con el refresh token (puede venir en header o body).
/// </summary>
public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = null!;
}

/// <summary>
/// Response después de login/register exitoso.
/// Se devuelven los tokens y datos básicos del usuario.
/// </summary>
public class AuthResponse
{
    public Guid UserId { get; set; }
    public string Email { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime AccessTokenExpiresAt { get; set; }
}

/// <summary>
/// Response después de refresh exitoso.
/// Se devuelven solo los nuevos tokens.
/// </summary>
public class TokenResponse
{
    public string AccessToken { get; set; } = null!;
    public string RefreshToken { get; set; } = null!;
    public DateTime AccessTokenExpiresAt { get; set; }
}
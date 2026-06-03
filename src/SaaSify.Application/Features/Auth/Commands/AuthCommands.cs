using SaaSify.Application.Common;
using SaaSify.Application.Features.Auth.Dtos;

namespace SaaSify.Application.Features.Auth.Commands;

/// <summary>
/// Command para registrar un nuevo developer.
/// 
/// El handler:
/// 1. Valida que el email no exista
/// 2. Hashea la password con BCrypt
/// 3. Crea el usuario en la base de datos
/// 4. Genera JWT + Refresh token
/// 5. Devuelve los tokens y datos del usuario
/// </summary>
public class RegisterUserCommand : Command<Result<AuthResponse>>
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
    public string Name { get; set; } = null!;
}

/// <summary>
/// Command para login.
/// 
/// El handler:
/// 1. Busca el usuario por email
/// 2. Verifica la password con BCrypt
/// 3. Genera JWT + Refresh token
/// 4. Guarda el refresh token en la base de datos
/// 5. Devuelve los tokens
/// </summary>
public class LoginUserCommand : Command<Result<AuthResponse>>
{
    public string Email { get; set; } = null!;
    public string Password { get; set; } = null!;
}

/// <summary>
/// Command para refrescar el JWT.
/// 
/// El handler:
/// 1. Valida que el refresh token exista y sea válido
/// 2. Verifica que no haya sido revocado
/// 3. Genera un nuevo JWT
/// 4. Opcionalmente, rota el refresh token (genera uno nuevo)
/// 5. Devuelve los nuevos tokens
/// </summary>
public class RefreshTokenCommand : Command<Result<TokenResponse>>
{
    public string RefreshToken { get; set; } = null!;
}

/// <summary>
/// Command para logout.
/// 
/// El handler revoca el refresh token en la base de datos.
/// El access token seguirá siendo válido hasta que expire,
/// pero al hacer refresh con el refresh token anterior, fallará.
/// </summary>
public class LogoutUserCommand : Command<Result>
{
    public Guid UserId { get; set; }
    public string RefreshToken { get; set; } = null!;
}
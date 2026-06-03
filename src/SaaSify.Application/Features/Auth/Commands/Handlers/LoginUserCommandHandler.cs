using MediatR;
using SaaSify.Application.Common;
using SaaSify.Application.Features.Auth.Commands;
using SaaSify.Application.Features.Auth.Dtos;
using SaaSify.Application.Interfaces;
using SaaSify.Domain.Interfaces;

namespace SaaSify.Application.Features.Auth.Commands.Handlers;

/// <summary>
/// Handler para LoginUserCommand.
/// 
/// Se encarga de:
/// 1. Buscar el usuario por email
/// 2. Verificar que la password es correcta
/// 3. Generar JWT + Refresh token
/// 4. Guardar el refresh token en la BD (para poder revocarlo después)
/// 5. Devolver los tokens
/// 
/// Patrón: validaciones tempranas devuelven Result.Failure.
/// </summary>
public class LoginUserCommandHandler : IRequestHandler<LoginUserCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    public LoginUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<AuthResponse>> Handle(
        LoginUserCommand request,
        CancellationToken cancellationToken)
    {
        // ── Paso 1: Buscar el usuario por email ────────────────────────────
        // Se consulta el repositorio. Si no existe, devuelve null.
        var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);

        if (user == null)
            return Result<AuthResponse>.Failure(
                "Email o password incorrectos",
                errorCode: 401); // Unauthorized
        
        // Se verifica que el usuario no esté suspendido o eliminado.
        if (user.Status != SaaSify.Domain.Enums.UserStatus.Active)
            return Result<AuthResponse>.Failure(
                "Usuario no está activo",
                errorCode: 403); // Forbidden

        // ── Paso 2: Verificar que la password es correcta ──────────────────
        // Se usa BCrypt para comparar la password con el hash guardado.
        var passwordValid = _passwordHasher.VerifyPassword(request.Password, user.PasswordHash);

        if (!passwordValid)
            return Result<AuthResponse>.Failure(
                "Email o password incorrectos",
                errorCode: 401);

        // ── Paso 3: Generar JWT + Refresh token ────────────────────────────
        var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Email);
        var refreshToken = _jwtTokenService.GenerateRefreshToken(user.Id);

        // ── Paso 4: Guardar el refresh token en la BD ──────────────────────
        // (Implementación futura: tabla RefreshTokens para poder revocar después)
        // Por ahora, el refresh token se valida verificando que sea un JWT válido.

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ── Paso 5: Devolver los tokens ────────────────────────────────────
        var response = new AuthResponse
        {
            UserId = user.Id,
            Email = user.Email,
            Name = user.Name,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };

        return Result<AuthResponse>.Success(response);
    }
}
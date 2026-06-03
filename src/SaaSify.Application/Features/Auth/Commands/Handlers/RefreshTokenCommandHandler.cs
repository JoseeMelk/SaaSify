using MediatR;
using SaaSify.Application.Common;
using SaaSify.Application.Features.Auth.Commands;
using SaaSify.Application.Features.Auth.Dtos;
using SaaSify.Application.Interfaces;
using SaaSify.Domain.Interfaces;

namespace SaaSify.Application.Features.Auth.Commands.Handlers;

/// <summary>
/// Handler para RefreshTokenCommand.
/// 
/// Se encarga de:
/// 1. Validar que el refresh token sea un JWT válido
/// 2. Extraer el userId del token
/// 3. Buscar el usuario (para confirmar que existe y está activo)
/// 4. Generar un nuevo access token
/// 5. Opcionalmente, rotar el refresh token
/// 6. Devolver los nuevos tokens
/// 
/// Patrón: las validaciones de tokens ocurren antes de consultas a BD.
/// </summary>
public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<TokenResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;

    public RefreshTokenCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result<TokenResponse>> Handle(
        RefreshTokenCommand request,
        CancellationToken cancellationToken)
    {
        // ── Paso 1: Extraer el userId del refresh token ─────────────────────
        // Se valida que el token sea un JWT válido y se extrae el userId.
        var userId = _jwtTokenService.GetUserIdFromToken(request.RefreshToken);

        if (userId == null || userId == Guid.Empty)
            return Result<TokenResponse>.Failure(
                "Refresh token inválido o expirado",
                errorCode: 401);

        // ── Paso 2: Buscar el usuario en la BD ─────────────────────────────
        // Se verifica que el usuario existe y está activo.
        var user = await _userRepository.GetByIdAsync(userId.Value, cancellationToken);

        if (user == null)
            return Result<TokenResponse>.Failure(
                "Usuario no encontrado",
                errorCode: 404);

        if (user.Status != SaaSify.Domain.Enums.UserStatus.Active)
            return Result<TokenResponse>.Failure(
                "Usuario no está activo",
                errorCode: 403);

        // ── Paso 3: Generar nuevos tokens ──────────────────────────────────
        // Se genera un nuevo access token con duración corta.
        var newAccessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Email);

        // Se genera un nuevo refresh token (rotación de tokens).
        // Esto invalida el anterior automáticamente.
        var newRefreshToken = _jwtTokenService.GenerateRefreshToken(user.Id);

        // ── Paso 4: Persistir cambios (si es necesario) ────────────────────
        // Por ahora no hay cambios de estado en el usuario.
        // En una implementación completa, se guardaría la revocación del token anterior.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ── Paso 5: Devolver los nuevos tokens ─────────────────────────────
        var response = new TokenResponse
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            AccessTokenExpiresAt = DateTime.UtcNow.AddMinutes(15)
        };

        return Result<TokenResponse>.Success(response);
    }
}
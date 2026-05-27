using MediatR;
using SaaSify.Application.Common;
using SaaSify.Application.Features.Auth.Commands;
using SaaSify.Application.Interfaces;
using SaaSify.Domain.Interfaces;

namespace SaaSify.Application.Features.Auth.Commands.Handlers;

/// <summary>
/// Handler para LogoutUserCommand.
/// 
/// Se encarga de:
/// 1. Validar que el usuario existe
/// 2. Validar que el refresh token pertenece al usuario
/// 3. Revocar el refresh token (agregarlo a una blacklist)
/// 4. Confirmar el logout
/// 
/// Patrón: el logout es idempotente — puede llamarse múltiples veces sin problemas.
/// </summary>
public class LogoutUserCommandHandler : IRequestHandler<LogoutUserCommand, Result>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IJwtTokenService _jwtTokenService;

    public LogoutUserCommandHandler(
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        IJwtTokenService jwtTokenService)
    {
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _jwtTokenService = jwtTokenService;
    }

    public async Task<Result> Handle(
        LogoutUserCommand request,
        CancellationToken cancellationToken)
    {
        // ── Paso 1: Validar que el usuario existe ──────────────────────────
        var user = await _userRepository.GetByIdAsync(request.UserId, cancellationToken);

        if (user == null)
            return Result.Failure("Usuario no encontrado", errorCode: 404);

        // ── Paso 2: Validar el refresh token ───────────────────────────────
        // Se extrae el userId del token para confirmar que pertenece al usuario.
        var tokenUserId = _jwtTokenService.GetUserIdFromToken(request.RefreshToken);

        if (tokenUserId != request.UserId)
            return Result.Failure(
                "Refresh token no válido para este usuario",
                errorCode: 401);

        // ── Paso 3: Revocar el refresh token ───────────────────────────────
        // (Implementación futura: agregar a blacklist en Redis o BD)
        // Por ahora, el logout simplemente valida que el token es correcto.
        // El token seguirá siendo técnicamente válido hasta que expire,
        // pero en una implementación completa se agregaría a una blacklist.

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ── Paso 4: Confirmar el logout ────────────────────────────────────
        return Result.Success();
    }
}
using MediatR;
using SaaSify.Application.Common;
using SaaSify.Application.Features.Auth.Commands;
using SaaSify.Application.Features.Auth.Dtos;
using SaaSify.Application.Interfaces;
using SaaSify.Domain.Entities;
using SaaSify.Domain.Interfaces;

namespace SaaSify.Application.Features.Auth.Commands.Handlers;

/// <summary>
/// Handler para RegisterUserCommand.
/// 
/// Se encarga de:
/// 1. Verificar que el email no esté registrado
/// 2. Hashear la password con BCrypt
/// 3. Crear la entidad User en el dominio
/// 4. Guardarla en la base de datos
/// 5. Generar JWT + Refresh token
/// 6. Devolver los tokens y datos del usuario
/// 
/// Patrón: cada paso verifica precondiciones y devuelve Result en caso de error.
/// </summary>
public class RegisterUserCommandHandler : IRequestHandler<RegisterUserCommand, Result<AuthResponse>>
{
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;

    /// <summary>
    /// Se inyectan las dependencias: repositorio, UoW, servicios de auth.
    /// Todas estas interfaces están definidas en Application/Domain/Infrastructure.
    /// </summary>
    public RegisterUserCommandHandler(
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
        RegisterUserCommand request,
        CancellationToken cancellationToken)
    {
        // ── Paso 1: Verificar que el email no esté registrado ────────────────
        // Se consulta el repositorio para saber si el email ya existe.
        var emailExists = await _userRepository.ExistsByEmailAsync(
            request.Email,
            cancellationToken);

        if (emailExists)
            return Result<AuthResponse>.Failure(
                "El email ya está registrado",
                errorCode: 409); // Conflict

        // ── Paso 2: Hashear la password ────────────────────────────────────
        // Se usa BCrypt para generar un hash seguro.
        // La password original NUNCA se guarda.
        var passwordHash = _passwordHasher.HashPassword(request.Password);

        // ── Paso 3: Crear la entidad User en el dominio ─────────────────────
        // Se usa el factory method del dominio para crear un User válido.
        // El constructor privado + Create() garantiza un objeto válido desde el inicio.
        var user = User.Create(request.Email, passwordHash, request.Name);

        // ── Paso 4: Guardar en la base de datos ────────────────────────────
        // Se agrega al repositorio.
        // Todavía no se guarda — eso ocurre en SaveChangesAsync() del UnitOfWork.
        await _userRepository.AddAsync(user, cancellationToken);

        // Se confirma la transacción en la base de datos.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ── Paso 5: Generar JWT + Refresh token ────────────────────────────
        // Se generan los tokens con la información del usuario creado.
        var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Email);
        var refreshToken = _jwtTokenService.GenerateRefreshToken(user.Id);

        // ── Paso 6: Devolver los tokens y datos del usuario ─────────────────
        // Se construye la response con los datos necesarios para el cliente.
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
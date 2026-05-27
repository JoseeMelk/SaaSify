using MediatR;
using Microsoft.AspNetCore.Mvc;
using SaaSify.Application.Features.Auth.Commands;
using SaaSify.Application.Features.Auth.Dtos;

namespace SaaSify.Api.Controllers;

/// <summary>
/// Controller para autenticación de developers.
/// 
/// Se exponen 4 endpoints:
/// - POST /api/auth/register — registrar nuevo developer
/// - POST /api/auth/login — login con credenciales
/// - POST /api/auth/refresh — renovar JWT con refresh token
/// - POST /api/auth/logout — logout y revocar refresh token
/// 
/// Patrón: el controller es delgado. MediatR orquesta la lógica.
/// </summary>

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    /// <summary>
    /// Se inyecta IMediator — el bus que orquesta commands y queries.
    /// </summary>
    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// POST /api/auth/register
    /// 
    /// Se registra un nuevo developer en SaaSify.
    /// La request contiene email, password y nombre.
    /// La response contiene los tokens de autenticación.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request, CancellationToken cancellationToken)
    {
        // Se convierte la request HTTP a un command del dominio.
        var command = new RegisterUserCommand
        {
            Email = request.Email,
            Password = request.Password,
            Name = request.Name
        };

        // Se envia el command al MediatR.
        // MediatR ejecuta: validadores -> handler -> devuelve Result<AuthResponse>
        var result = await _mediator.Send(command, cancellationToken);
        // Se verifica si el resultado es exitoso.
        if (!result.IsSuccess) return BadRequest(new { Error = result.Error });

        // Se devuelve la respuesta con los datos y tokens.
        // Status 201 (Created) porque se creó un nuevo recurso (el usuario).
        return CreatedAtAction(nameof(Register), result.Data);
    }

    /// <summary>
    /// POST /api/auth/login
    /// 
    /// Se autentica un developer existente.
    /// La request contiene email y password.
    /// La response contiene los tokens de autenticación.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginUserRequest request, CancellationToken cancellationToken)
    {
        var command = new LoginUserCommand
        {
            Email = request.Email,
            Password = request.Password
        };

        var result = await _mediator.Send(command, cancellationToken);
        if (!result.IsSuccess) return Unauthorized(new { Error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// POST /api/auth/refresh
    /// 
    /// Se renueva el JWT access token usando un refresh token válido.
    /// El refresh token puede venir en la request body o en un header.
    /// La response contiene los nuevos tokens.
    /// </summary>
    [HttpPost("refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var command = new RefreshTokenCommand
        {
            RefreshToken = request.RefreshToken
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess) return Unauthorized(new { Error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// POST /api/auth/logout
    /// 
    /// Se realiza el logout revocando el refresh token.
    /// El developer envía su userId y refresh token.
    /// La operación siempre devuelve 200 (es idempotente).
    /// </summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutUserRequest request, CancellationToken cancellationToken)
    {
        var command = new LogoutUserCommand
        {
            UserId = request.UserId,
            RefreshToken = request.RefreshToken
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess) return BadRequest(new { Error = result.Error });
        
        return Ok( new { Message = "Logout successful" });
    }
}
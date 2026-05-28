using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSify.Application.Features.Projects.Commands;
using SaaSify.Application.Features.Projects.Dtos;
using SaaSify.Application.Features.Projects.Queries;

namespace SaaSify.Api.Controllers;

/// <summary>
/// Controller para gestión de proyectos del developer.
/// 
/// Todos los endpoints requieren autenticación JWT — [Authorize].
/// El OwnerId se extrae del token, nunca del body del request.
/// </summary>
[ApiController]
[Route("api/projects")]
[Authorize] // todos los endpoints requieren JWT válido
public class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Se obtiene el OwnerId del JWT del developer autenticado.
    /// 
    /// El claim "userId" fue puesto en el token al hacer login.
    /// ASP.NET lo expone en HttpContext.User.Claims.
    /// Si el claim no existe o es inválido, devuelve Guid.Empty.
    /// </summary>
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    /// <summary>
    /// POST /api/projects
    /// 
    /// Se crea un nuevo proyecto para el developer autenticado.
    /// La API key se devuelve solo en esta response — guardarla de inmediato.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateProjectCommand
        {
            OwnerId = GetCurrentUserId(),
            Name = request.Name,
            Slug = request.Slug
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(result.ErrorCode ?? 400, new { error = result.Error });

        // 201 Created — se creó un nuevo recurso
        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    /// <summary>
    /// GET /api/projects/{id}
    /// 
    /// Se obtiene un proyecto por su ID.
    /// Solo el owner puede ver su propio proyecto.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var query = new GetProjectByIdQuery
        {
            ProjectId = id,
            OwnerId = GetCurrentUserId()
        };

        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(result.ErrorCode ?? 400, new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// GET /api/projects
    /// 
    /// Se listan todos los proyectos del developer autenticado.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var query = new ListProjectsQuery
        {
            OwnerId = GetCurrentUserId()
        };

        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(result.ErrorCode ?? 400, new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// POST /api/projects/{id}/rotate-api-key
    /// 
    /// Se genera una nueva API key para el proyecto.
    /// La key anterior queda invalidada inmediatamente.
    /// La nueva key se devuelve solo en esta response.
    /// </summary>
    [HttpPost("{id:guid}/rotate-api-key")]
    public async Task<IActionResult> RotateApiKey(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new RotateApiKeyCommand
        {
            ProjectId = id,
            OwnerId = GetCurrentUserId()
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(result.ErrorCode ?? 400, new { error = result.Error });

        return Ok(result.Data);
    }
}
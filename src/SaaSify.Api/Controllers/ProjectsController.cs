using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSify.Api.Authorization;
using SaaSify.Application.Features.Projects.Commands;
using SaaSify.Application.Features.Projects.Dtos;
using SaaSify.Application.Features.Projects.Queries;

namespace SaaSify.Api.Controllers;

/// <summary>
/// Controller para gestión de proyectos del developer.
/// 
/// Se usa la policy RequireAccessToken en vez de [Authorize] simple.
/// Esto garantiza que solo access tokens son aceptados —
/// los refresh tokens son rechazados con 403.
/// </summary>
[ApiController]
[Route("api/projects")]
[Authorize(Policy = Policies.RequireAccessToken)]
public class ProjectsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProjectsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

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

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

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
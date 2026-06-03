using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSify.Api.Authorization;
using SaaSify.Api.Extensions;
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

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateProjectRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateProjectCommand
        {
            OwnerId = User.GetUserId(),
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
            OwnerId = User.GetUserId()
        };

        var result = await _mediator.Send(query, cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpGet]
    public async Task<IActionResult> List(CancellationToken cancellationToken)
    {
        var query = new ListProjectsQuery
        {
            OwnerId = User.GetUserId()
        };

        var result = await _mediator.Send(query, cancellationToken);

        return result.ToActionResult(this);
    }

    [HttpPost("{id:guid}/rotate-api-key")]
    public async Task<IActionResult> RotateApiKey(
        Guid id,
        CancellationToken cancellationToken)
    {
        var command = new RotateApiKeyCommand
        {
            ProjectId = id,
            OwnerId = User.GetUserId()
        };

        var result = await _mediator.Send(command, cancellationToken);

        return result.ToActionResult(this);
    }
}
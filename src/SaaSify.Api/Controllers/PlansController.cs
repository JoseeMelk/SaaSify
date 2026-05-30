using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSify.Api.Authorization;
using SaaSify.Application.Features.Plans.Commands;
using SaaSify.Application.Features.Plans.Dtos;
using SaaSify.Application.Features.Plans.Queries;

namespace SaaSify.Api.Controllers;

/// <summary>
/// Controller para gestión de planes dentro de un proyecto.
/// 
/// Se usa la policy RequireAccessToken en vez de [Authorize] simple.
/// Esto garantiza que solo access tokens son aceptados —
/// los refresh tokens son rechazados con 403.
/// </summary>
[ApiController]
[Route("api/projects/{projectId:guid}/plans")]
[Authorize(Policy = Policies.RequireAccessToken)]
public class PlansController : ControllerBase
{
    private readonly IMediator _mediator;

    public PlansController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    /// <summary>
    /// Crea un nuevo plan para el proyecto.
    /// 
    /// POST /api/projects/{projectId}/plans
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Create(
        Guid projectId,
        [FromBody] CreatePlanRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreatePlanCommand
        {
            OwnerId = GetCurrentUserId(),
            ProjectId = projectId,
            Name = request.Name,
            Slug = request.Slug,
            Price = request.Price,
            Currency = request.Currency,
            BillingCycle = request.BillingCycle,
            IsPublic = request.IsPublic
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(result.ErrorCode ?? 400, new { error = result.Error });

        return CreatedAtAction(nameof(GetBySlug), new { projectId, slug = result.Data!.Slug }, result.Data);
    }

    /// <summary>
    /// Obtiene un plan específico por su slug.
    /// 
    /// GET /api/projects/{projectId}/plans/{slug}
    /// </summary>
    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(
        Guid projectId,
        string slug,
        CancellationToken cancellationToken)
    {
        var query = new GetPlanBySlugQuery
        {
            OwnerId = GetCurrentUserId(),
            ProjectId = projectId,
            Slug = slug
        };

        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(result.ErrorCode ?? 400, new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Lista todos los planes del proyecto.
    /// 
    /// GET /api/projects/{projectId}/plans
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> List(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var query = new ListPlansQuery
        {
            OwnerId = GetCurrentUserId(),
            ProjectId = projectId
        };

        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(result.ErrorCode ?? 400, new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// Desactiva un plan.
    /// 
    /// POST /api/projects/{projectId}/plans/{planId}/deactivate
    /// </summary>
    [HttpPost("{planId:guid}/deactivate")]
    public async Task<IActionResult> Deactivate(
        Guid projectId,
        Guid planId,
        CancellationToken cancellationToken)
    {
        var command = new DeactivatePlanCommand
        {
            Id = planId,
            OwnerId = GetCurrentUserId(),
            ProjectId = projectId
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(result.ErrorCode ?? 400, new { error = result.Error });

        return Ok();
    }
}

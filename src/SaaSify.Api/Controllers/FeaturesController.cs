using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSify.Api.Authorization;
using SaaSify.Application.Features.Plans.Features.Commands;
using SaaSify.Application.Features.Plans.Features.Dtos;
using SaaSify.Application.Features.Plans.Features.Queries;

namespace SaaSify.Api.Controllers;

/// <summary>
/// Controller para gestión de features dentro de un plan.
/// 
/// Las features viven dentro de un plan — la ruta refleja esa jerarquía:
/// /api/projects/{projectId}/plans/{planId}/features
/// 
/// El GET usa planSlug en vez de planId — más cómodo para el developer.
/// </summary>
[ApiController]
[Authorize(Policy = Policies.RequireAccessToken)]
public class FeaturesController : ControllerBase
{
    private readonly IMediator _mediator;

    public FeaturesController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    /// <summary>
    /// POST /api/projects/{projectId}/plans/{planId}/features
    /// Agrega una feature a un plan.
    /// </summary>
    [HttpPost("api/projects/{projectId:guid}/plans/{planId:guid}/features")]
    public async Task<IActionResult> Create(
        Guid projectId,
        Guid planId,
        [FromBody] CreateFeatureRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CreateFeatureCommand
        {
            OwnerId = GetCurrentUserId(),
            ProjectId = projectId,
            PlanId = planId,
            Slug = request.Slug,
            IsEnabled = request.IsEnabled
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(result.ErrorCode ?? 400, new { error = result.Error });

        return StatusCode(201, result.Data);
    }

    /// <summary>
    /// GET /api/projects/{projectId}/plans/{planSlug}/features
    /// Lista todas las features de un plan por su slug.
    /// </summary>
    [HttpGet("api/projects/{projectId:guid}/plans/{planSlug}/features")]
    public async Task<IActionResult> List(
        Guid projectId,
        string planSlug,
        CancellationToken cancellationToken)
    {
        var query = new ListFeaturesQuery
        {
            OwnerId = GetCurrentUserId(),
            ProjectId = projectId,
            PlanSlug = planSlug
        };

        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(result.ErrorCode ?? 400, new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// DELETE /api/projects/{projectId}/plans/{planId}/features/{featureId}
    /// Elimina una feature de un plan.
    /// </summary>
    [HttpDelete("api/projects/{projectId:guid}/plans/{planId:guid}/features/{featureId:guid}")]
    public async Task<IActionResult> Remove(
        Guid projectId,
        Guid planId,
        Guid featureId,
        CancellationToken cancellationToken)
    {
        var command = new RemoveFeatureCommand
        {
            OwnerId = GetCurrentUserId(),
            ProjectId = projectId,
            PlanId = planId,
            FeatureId = featureId
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(result.ErrorCode ?? 400, new { error = result.Error });

        return NoContent();
    }
}
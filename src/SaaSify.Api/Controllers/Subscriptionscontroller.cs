using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSify.Api.Authorization;
using SaaSify.Application.Features.Subscriptions.Commands;
using SaaSify.Application.Features.Subscriptions.Dtos;
using SaaSify.Application.Features.Subscriptions.Queries;

namespace SaaSify.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/customers/{externalId}/subscriptions")]
[Authorize(Policy = Policies.RequireAccessToken)]
public class SubscriptionsController : ControllerBase
{
    private readonly IMediator _mediator;

    public SubscriptionsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst("userId")?.Value;
        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }

    /// <summary>
    /// GET /api/projects/{projectId}/customers/{externalId}/subscriptions/active
    /// Obtiene la suscripción activa del customer.
    /// </summary>
    [HttpGet("active")]
    public async Task<IActionResult> GetActive(
        Guid projectId,
        string externalId,
        CancellationToken cancellationToken)
    {
        var query = new GetActiveSubscriptionQuery
        {
            OwnerId = GetCurrentUserId(),
            ProjectId = projectId,
            ExternalId = externalId
        };

        var result = await _mediator.Send(query, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(result.ErrorCode ?? 400, new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// POST /api/projects/{projectId}/customers/{externalId}/subscriptions/assign
    /// Asigna o cambia el plan del customer.
    /// Cancela la suscripción activa y crea una nueva.
    /// </summary>
    [HttpPost("assign")]
    public async Task<IActionResult> Assign(
        Guid projectId,
        string externalId,
        [FromBody] AssignPlanRequest request,
        CancellationToken cancellationToken)
    {
        var command = new AssignPlanCommand
        {
            OwnerId = GetCurrentUserId(),
            ProjectId = projectId,
            ExternalId = externalId,
            PlanSlug = request.PlanSlug,
            BillingCycle = request.BillingCycle,
            PaymentMethod = request.PaymentMethod,
            ExternalPaymentRef = request.ExternalPaymentRef
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(result.ErrorCode ?? 400, new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// POST /api/projects/{projectId}/customers/{externalId}/subscriptions/renew
    /// El developer notifica que el cobro fue exitoso.
    /// SaaSify actualiza CurrentPeriodEnd y RenewsAt.
    /// </summary>
    [HttpPost("renew")]
    public async Task<IActionResult> Renew(
        Guid projectId,
        string externalId,
        [FromBody] RenewSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new RenewSubscriptionCommand
        {
            OwnerId = GetCurrentUserId(),
            ProjectId = projectId,
            ExternalId = externalId,
            ExternalPaymentRef = request.ExternalPaymentRef
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(result.ErrorCode ?? 400, new { error = result.Error });

        return Ok(result.Data);
    }

    /// <summary>
    /// POST /api/projects/{projectId}/customers/{externalId}/subscriptions/cancel
    /// Cancela la suscripción activa.
    /// Immediately = true → pierde acceso ahora.
    /// Immediately = false → mantiene acceso hasta CurrentPeriodEnd.
    /// </summary>
    [HttpPost("cancel")]
    public async Task<IActionResult> Cancel(
        Guid projectId,
        string externalId,
        [FromBody] CancelSubscriptionRequest request,
        CancellationToken cancellationToken)
    {
        var command = new CancelSubscriptionCommand
        {
            OwnerId = GetCurrentUserId(),
            ProjectId = projectId,
            ExternalId = externalId,
            Immediately = request.Immediately
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(result.ErrorCode ?? 400, new { error = result.Error });

        return Ok(result.Data);
    }
}
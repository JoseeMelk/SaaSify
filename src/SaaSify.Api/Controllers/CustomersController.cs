using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSify.Api.Authorization;
using SaaSify.Application.Features.Customers.Dtos;
using SaaSify.Application.Features.Customers.Queries;
using SaaSify.Application.Features.Customers.Commands;
using SaaSify.Api.Extensions;

namespace SaaSify.Api.Controllers;

[ApiController]
[Route("api/projects/{projectId}/customers")]
[Authorize(Policy = Policies.RequireAccessToken)]
public class CustomersController : ControllerBase
{
    private readonly IMediator _mediator;

    public CustomersController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<IActionResult> Create(
    Guid projectId,               // minúscula — convención C#
    [FromBody] CreateCustomerRequest request,
    CancellationToken cancellationToken)
    {
        var command = new CreateCustomerCommand
        {
            OwnerId = User.GetUserId(),
            ProjectId = projectId,
            ExternalId = request.ExternalId,
            Name = request.Name,
            Email = request.Email,
            PlanSlug = request.PlanSlug
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.IsSuccess)
            return StatusCode(result.ErrorCode ?? 400, new { error = result.Error });

        // 201 Created — se creó un nuevo recurso
        // CreatedAtAction apunta al endpoint GET que devuelve este customer
        // result.Data es el CustomerResponse con todos los datos
        return CreatedAtAction(
            nameof(GetByExternalId),                          // nombre del método GET
            new { projectId, externalId = result.Data!.ExternalId }, // parámetros de la ruta
            result.Data);                                     // el objeto creado
    }

    [HttpGet("{externalId}")]
    public async Task<IActionResult> GetByExternalId(
    Guid projectId,
    string externalId,
    CancellationToken cancellationToken)
    {
        var query = new GetCustomerByExternalIdQuery
        {
            OwnerId = User.GetUserId(),
            ProjectId = projectId,
            ExternalId = externalId
        };

        var result = await _mediator.Send(query, cancellationToken);

        // 200 OK — devuelve CustomerResponse con ActiveSubscription incluida
        return result.ToActionResult(this);
    }

    [HttpGet]
    public async Task<IActionResult> List(
        Guid projectId,
        CancellationToken cancellationToken)
    {
        var query = new ListCustomersQuery
        {
            OwnerId = User.GetUserId(),
            ProjectId = projectId
        };

        var result = await _mediator.Send(query, cancellationToken);

        // 200 OK — devuelve lista de CustomerListResponse (sin ActiveSubscription)
        return result.ToActionResult(this);
    }
}
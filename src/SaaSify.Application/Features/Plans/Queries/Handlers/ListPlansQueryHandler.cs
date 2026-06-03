using MediatR;
using SaaSify.Application.Common;
using SaaSify.Application.Features.Plans.Dtos;
using SaaSify.Application.Features.Plans.Queries;
using SaaSify.Domain.Interfaces;

namespace SaaSify.Application.Features.Plans.Queries.Handlers;

/// <summary>
/// Handler para ListPlansQuery.
/// 
/// Se encarga de:
/// 1. Verificar que el proyecto existe y pertenece al owner
/// 2. Obtener todos los planes del proyecto
/// 3. Mapear cada plan a PlanResponse con features incluidas
/// 4. Devolver lista ordenada
/// 
/// Patrón: cada developer solo ve los planes de sus propios proyectos
/// La lista incluye todos los planes (activos e inactivos)
/// </summary>
public class ListPlansQueryHandler : IRequestHandler<ListPlansQuery, Result<IReadOnlyList<PlanResponse>>>
{
    private readonly IPlanRepository _planRepository;
    private readonly IProjectRepository _projectRepository;

    public ListPlansQueryHandler(
        IPlanRepository planRepository,
        IProjectRepository projectRepository)
    {
        _planRepository = planRepository;
        _projectRepository = projectRepository;
    }

    public async Task<Result<IReadOnlyList<PlanResponse>>> Handle(
        ListPlansQuery request,
        CancellationToken cancellationToken)
    {
        // ── Paso 1: Verificar que el proyecto existe ─────────────────────
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);

        if (project == null)
            return Result<IReadOnlyList<PlanResponse>>.Failure(
                "Project not found",
                errorCode: 404);

        // ── Paso 2: Verificar que el proyecto pertenece al owner ─────────
        if (project.OwnerId != request.OwnerId)
            return Result<IReadOnlyList<PlanResponse>>.Failure(
                "You don't have permission to view plans in this project",
                errorCode: 403); // Forbidden

        // ── Paso 3: Obtener todos los planes del proyecto ────────────────
        var plans = await _planRepository.GetByProjectIdAsync(
            request.ProjectId,
            cancellationToken);

        // ── Paso 4: Mapear cada plan a PlanResponse ──────────────────────
        var responses = plans
            .Select(MapToPlanResponse)
            .ToList()
            .AsReadOnly();

        // ── Paso 5: Devolver el resultado ────────────────────────────────
        return Result<IReadOnlyList<PlanResponse>>.Success(responses);
    }

    /// <summary>
    /// Mapea una entidad Plan a PlanResponse.
    /// </summary>
    private static PlanResponse MapToPlanResponse(Domain.Entities.Plan plan)
    {
        return new PlanResponse
        {
            Id = plan.Id,
            ProjectId = plan.ProjectId,
            Name = plan.Name,
            Slug = plan.Slug,
            Price = plan.Price,
            Currency = plan.Currency,
            BillingCycle = plan.BillingCycle?.ToString(),
            IsActive = plan.IsActive,
            IsPublic = plan.IsPublic,
            CreatedAt = plan.CreatedAt,
            UpdatedAt = plan.UpdatedAt,
            Features = plan.Features
                .Select(f => new FeatureResponse
                {
                    Id = f.Id,
                    Slug = f.Slug,
                    IsEnabled = f.IsEnabled
                })
                .ToList()
                .AsReadOnly()
        };
    }
}

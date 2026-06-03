using MediatR;
using SaaSify.Application.Common;
using SaaSify.Application.Features.Plans.Dtos;
using SaaSify.Application.Features.Plans.Queries;
using SaaSify.Domain.Interfaces;

namespace SaaSify.Application.Features.Plans.Queries.Handlers;

/// <summary>
/// Handler para GetPlanBySlugQuery.
/// 
/// Se encarga de:
/// 1. Verificar que el proyecto existe y pertenece al owner
/// 2. Obtener el plan por slug dentro de ese proyecto
/// 3. Mapear a PlanResponse con features incluidas
/// 4. Devolver el plan
/// 
/// Patrón: solo el owner puede ver los planes de su proyecto
/// </summary>
public class GetPlanBySlugQueryHandler : IRequestHandler<GetPlanBySlugQuery, Result<PlanResponse>>
{
    private readonly IPlanRepository _planRepository;
    private readonly IProjectRepository _projectRepository;

    public GetPlanBySlugQueryHandler(
        IPlanRepository planRepository,
        IProjectRepository projectRepository)
    {
        _planRepository = planRepository;
        _projectRepository = projectRepository;
    }

    public async Task<Result<PlanResponse>> Handle(
        GetPlanBySlugQuery request,
        CancellationToken cancellationToken)
    {
        // ── Paso 1: Verificar que el proyecto existe ─────────────────────
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);

        if (project == null)
            return Result<PlanResponse>.Failure(
                "Project not found",
                errorCode: 404);

        // ── Paso 2: Verificar que el proyecto pertenece al owner ─────────
        if (project.OwnerId != request.OwnerId)
            return Result<PlanResponse>.Failure(
                "You don't have permission to view plans in this project",
                errorCode: 403); // Forbidden

        // ── Paso 3: Obtener el plan por slug dentro del proyecto ────────
        var plan = await _planRepository.GetBySlugAsync(
            request.ProjectId,
            request.Slug,
            cancellationToken);

        if (plan == null)
            return Result<PlanResponse>.Failure(
                "Plan not found",
                errorCode: 404);

        // ── Paso 4: Mapear a PlanResponse ────────────────────────────────
        var response = MapToPlanResponse(plan);

        // ── Paso 5: Devolver el resultado ────────────────────────────────
        return Result<PlanResponse>.Success(response);
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

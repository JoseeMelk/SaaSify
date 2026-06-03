using MediatR;
using SaaSify.Application.Common;
using SaaSify.Application.Features.Plans.Features.Dtos;
using SaaSify.Application.Features.Plans.Features.Queries;
using SaaSify.Domain.Interfaces;

namespace SaaSify.Application.Features.Plans.Features.Queries.Handlers;

public class ListFeaturesQueryHandler
    : IRequestHandler<ListFeaturesQuery, Result<IReadOnlyList<FeatureResponse>>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IPlanRepository _planRepository;

    public ListFeaturesQueryHandler(
        IProjectRepository projectRepository,
        IPlanRepository planRepository)
    {
        _projectRepository = projectRepository;
        _planRepository = planRepository;
    }

    public async Task<Result<IReadOnlyList<FeatureResponse>>> Handle(
        ListFeaturesQuery request,
        CancellationToken cancellationToken)
    {
        // ── Paso 1: Verificar proyecto y owner ────────────────────────────
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);

        if (project == null)
            return Result<IReadOnlyList<FeatureResponse>>.Failure("Project not found", errorCode: 404);

        if (project.OwnerId != request.OwnerId)
            return Result<IReadOnlyList<FeatureResponse>>.Failure("Unauthorized", errorCode: 403);

        // ── Paso 2: Obtener el plan por slug con sus features ─────────────
        // GetBySlugAsync ya incluye las features (Include en el repositorio).
        var plan = await _planRepository.GetBySlugAsync(request.ProjectId, request.PlanSlug, cancellationToken);

        if (plan == null)
            return Result<IReadOnlyList<FeatureResponse>>.Failure("Plan not found", errorCode: 404);

        // ── Paso 3: Mapear y devolver ─────────────────────────────────────
        var features = plan.Features
            .Select(f => new FeatureResponse
            {
                Id = f.Id,
                PlanId = f.PlanId,
                Slug = f.Slug,
                IsEnabled = f.IsEnabled,
                CreatedAt = f.CreatedAt,
                UpdatedAt = f.UpdatedAt
            })
            .ToList()
            .AsReadOnly();

        return Result<IReadOnlyList<FeatureResponse>>.Success(features);
    }
}
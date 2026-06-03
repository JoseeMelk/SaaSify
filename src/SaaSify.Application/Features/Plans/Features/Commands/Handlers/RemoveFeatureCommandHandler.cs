using MediatR;
using SaaSify.Application.Common;
using SaaSify.Application.Features.Plans.Features.Commands;
using SaaSify.Domain.Interfaces;

namespace SaaSify.Application.Features.Plans.Features.Commands.Handlers;

/// <summary>
/// Handler para RemoveFeatureCommand.
/// 
/// Se elimina una feature de un plan.
/// Feature no tiene repositorio propio — se opera a través del Plan.
/// 
/// EF Core trackea las features como parte del aggregate Plan.
/// Al hacer Update(plan) y SaveChanges, EF detecta que la feature
/// fue removida de la colección y la elimina de la BD.
/// </summary>
public class RemoveFeatureCommandHandler : IRequestHandler<RemoveFeatureCommand, Result>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IPlanRepository _planRepository;
    private readonly IUnitOfWork _unitOfWork;

    public RemoveFeatureCommandHandler(
        IProjectRepository projectRepository,
        IPlanRepository planRepository,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _planRepository = planRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        RemoveFeatureCommand request,
        CancellationToken cancellationToken)
    {
        // ── Paso 1: Verificar proyecto y owner ────────────────────────────
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);

        if (project == null)
            return Result.Failure("Project not found", errorCode: 404);

        if (project.OwnerId != request.OwnerId)
            return Result.Failure("Unauthorized", errorCode: 403);

        // ── Paso 2: Obtener el plan con sus features ───────────────────────
        var plan = await _planRepository.GetByIdWithFeaturesAsync(request.PlanId, cancellationToken);

        if (plan == null)
            return Result.Failure("Plan not found", errorCode: 404);

        if (plan.ProjectId != request.ProjectId)
            return Result.Failure("Plan does not belong to this project", errorCode: 400);

        // ── Paso 3: Verificar que la feature existe en el plan ────────────
        var feature = plan.Features.FirstOrDefault(f => f.Id == request.FeatureId);

        if (feature == null)
            return Result.Failure("Feature not found in this plan", errorCode: 404);

        // ── Paso 4: Eliminar la feature del plan ──────────────────────────
        // Se hace soft delete — igual que el resto del sistema.
        plan.RemoveFeature(request.FeatureId);

        _planRepository.Update(plan);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
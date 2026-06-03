using MediatR;
using SaaSify.Application.Common;
using SaaSify.Application.Features.Plans.Commands;
using SaaSify.Domain.Interfaces;

namespace SaaSify.Application.Features.Plans.Commands.Handlers;

/// <summary>
/// Handler para DeactivatePlanCommand.
/// 
/// Se encarga de:
/// 1. Verificar que el plan existe
/// 2. Verificar que el proyecto existe y pertenece al owner
/// 3. Verificar que el plan pertenece al proyecto
/// 4. Llamar al método Deactivate() del dominio
/// 5. Guardar en la base de datos
/// 6. Devolver resultado
/// 
/// Nota: Un plan NO puede desactivarse si tiene suscripciones activas.
/// El método Deactivate() del dominio lanza BusinessRuleException en ese caso.
/// </summary>
public class DeactivatePlanCommandHandler : IRequestHandler<DeactivatePlanCommand, Result>
{
    private readonly IPlanRepository _planRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeactivatePlanCommandHandler(
        IPlanRepository planRepository,
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork)
    {
        _planRepository = planRepository;
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result> Handle(
        DeactivatePlanCommand request,
        CancellationToken cancellationToken)
    {
        // ── Paso 1: Verificar que el proyecto existe y pertenece al owner ─
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);

        if (project == null)
            return Result.Failure(
                "Project not found",
                errorCode: 404);

        if (project.OwnerId != request.OwnerId)
            return Result.Failure(
                "You don't have permission to deactivate plans in this project",
                errorCode: 403); // Forbidden

        // ── Paso 2: Verificar que el plan existe ──────────────────────────
        var plan = await _planRepository.GetByIdAsync(request.Id, cancellationToken);

        if (plan == null)
            return Result.Failure(
                "Plan not found",
                errorCode: 404);

        // ── Paso 3: Verificar que el plan pertenece al proyecto ───────────
        if (plan.ProjectId != request.ProjectId)
            return Result.Failure(
                "The plan does not belong to this project",
                errorCode: 400); // Bad Request

        // ── Paso 4: Desactivar el plan ───────────────────────────────────
        // El método Deactivate() valida que no haya suscripciones activas.
        // Si hay, lanza BusinessRuleException que se captura aquí.
        try
        {
            plan.Deactivate();
        }
        catch (SaaSify.Domain.Exceptions.BusinessRuleException ex)
        {
            // Un plan con suscripciones activas no puede desactivarse
            return Result.Failure(
                ex.Message,
                errorCode: 422); // Unprocessable Entity
        }
        
        _planRepository.Update(plan);

        // ── Paso 5: Guardar en la base de datos ──────────────────────────
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ── Paso 6: Devolver resultado ────────────────────────────────────
        return Result.Success();
    }
}

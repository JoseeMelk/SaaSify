using MediatR;
using SaaSify.Application.Common;
using SaaSify.Application.Features.Plans.Features.Commands;
using SaaSify.Application.Features.Plans.Features.Dtos;
using SaaSify.Domain.Interfaces;

namespace SaaSify.Application.Features.Plans.Features.Commands.Handlers;

/// <summary>
/// Handler para CreateFeatureCommand.
/// 
/// Feature es owned por Plan — su ciclo de vida está controlado
/// por el aggregate Plan. Se opera a través de IPlanRepository,
/// no de un repositorio de Feature propio.
/// 
/// Se encarga de:
/// 1. Verificar que el proyecto existe y pertenece al owner
/// 2. Obtener el plan con sus features (Include)
/// 3. Verificar que el slug no exista ya en ese plan
/// 4. Llamar a plan.AddFeature() — el dominio controla la operación
/// 5. Guardar y devolver la feature creada
/// </summary>
public class CreateFeatureCommandHandler : IRequestHandler<CreateFeatureCommand, Result<FeatureResponse>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IPlanRepository _planRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateFeatureCommandHandler(
        IProjectRepository projectRepository,
        IPlanRepository planRepository,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _planRepository = planRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<FeatureResponse>> Handle(
        CreateFeatureCommand request,
        CancellationToken cancellationToken)
    {
        // ── Paso 1: Verificar proyecto y owner ────────────────────────────
        var project = await _projectRepository.GetByIdAsync(request.ProjectId, cancellationToken);

        if (project == null)
            return Result<FeatureResponse>.Failure("Project not found", errorCode: 404);

        if (project.OwnerId != request.OwnerId)
            return Result<FeatureResponse>.Failure("Unauthorized", errorCode: 403);

        // ── Paso 2: Obtener el plan con sus features ───────────────────────
        // Se usa GetByIdWithFeaturesAsync para que plan.Features no llegue vacío.
        // Sin el Include, plan.AddFeature() no puede detectar slugs duplicados.
        var plan = await _planRepository.GetByIdWithFeaturesAsync(request.PlanId, cancellationToken);

        if (plan == null)
            return Result<FeatureResponse>.Failure("Plan not found", errorCode: 404);

        // ── Paso 3: Verificar que el plan pertenece al proyecto ───────────
        if (plan.ProjectId != request.ProjectId)
            return Result<FeatureResponse>.Failure("Plan does not belong to this project", errorCode: 400);

        // ── Paso 4: Agregar la feature a través del aggregate ─────────────
        // plan.AddFeature() valida que el slug no esté duplicado.
        // Si ya existe, lanza BusinessRuleException.
        try
        {
            plan.AddFeature(request.Slug, request.IsEnabled);
        }
        catch (Domain.Exceptions.BusinessRuleException ex)
        {
            return Result<FeatureResponse>.Failure(ex.Message, errorCode: 409);
        }

        // ── Paso 5: Guardar ───────────────────────────────────────────────
        _planRepository.Update(plan);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ── Paso 6: Obtener la feature recién creada ──────────────────────
        // Se busca por slug porque es el identificador conocido.
        var created = plan.Features.First(f => f.Slug == request.Slug.ToLowerInvariant());

        return Result<FeatureResponse>.Success(new FeatureResponse
        {
            Id = created.Id,
            PlanId = created.PlanId,
            Slug = created.Slug,
            IsEnabled = created.IsEnabled,
            CreatedAt = created.CreatedAt,
            UpdatedAt = created.UpdatedAt
        });
    }
}
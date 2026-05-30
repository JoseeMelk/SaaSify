using MediatR;
using SaaSify.Application.Common;
using SaaSify.Application.Features.Plans.Commands;
using SaaSify.Application.Features.Plans.Dtos;
using SaaSify.Domain.Entities;
using SaaSify.Domain.Enums;
using SaaSify.Domain.Interfaces;

namespace SaaSify.Application.Features.Plans.Commands.Handlers;

/// <summary>
/// Handler para CreatePlanCommand.
/// 
/// Se encarga de:
/// 1. Verificar que el proyecto existe y pertenece al owner
/// 2. Generar slug desde el nombre si no se proporciona
/// 3. Validar que el slug sea único en ese proyecto
/// 4. Parsear BillingCycle de string a enum
/// 5. Crear la entidad Plan con validaciones de dominio
/// 6. Guardar en la base de datos
/// 7. Mapear a PlanResponse y devolver
/// 
/// Patrón: igual que CreateProjectCommandHandler
/// </summary>
public class CreatePlanCommandHandler : IRequestHandler<CreatePlanCommand, Result<PlanResponse>>
{
    private readonly IPlanRepository _planRepository;
    private readonly IProjectRepository _projectRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreatePlanCommandHandler(
        IPlanRepository planRepository,
        IProjectRepository projectRepository,
        IUnitOfWork unitOfWork)
    {
        _planRepository = planRepository;
        _projectRepository = projectRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<PlanResponse>> Handle(
        CreatePlanCommand request,
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
                "You do not have permission to create plans in this project",
                errorCode: 403); // Forbidden

        // ── Paso 3: Generar slug desde el nombre si no viene ────────────
        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? GenerateSlugFromName(request.Name)
            : NormalizeSlug(request.Slug);

        // ── Paso 4: Verificar que el slug sea único en ese proyecto ─────
        // Se buscan planes en el mismo proyecto con el mismo slug
        var slugExists = await _planRepository.ExistsBySlugAsync(
            request.ProjectId,
            slug,
            cancellationToken);

        if (slugExists)
            return Result<PlanResponse>.Failure(
                $"The slug '{slug}' is already in use in this project",
                errorCode: 409); // Conflict

        // ── Paso 5: Parsear BillingCycle de string a enum ──────────────
        BillingCycle? billingCycle = null;
        
        if (!string.IsNullOrWhiteSpace(request.BillingCycle))
        {
            // Intentar parsear el BillingCycle (Monthly, Yearly, etc)
            if (Enum.TryParse<BillingCycle>(request.BillingCycle, ignoreCase: true, out var parsed))
            {
                billingCycle = parsed;
            }
            else
            {
                return Result<PlanResponse>.Failure(
                    $"BillingCycle '{request.BillingCycle}' is not valid. Allowed values: {string.Join(", ", Enum.GetNames(typeof(BillingCycle)))}",
                    errorCode: 400); // Bad Request
            }
        }

        // ── Paso 6: Validar que Currency es requerido si Price > 0 ─────
        if (request.Price.HasValue && request.Price > 0 && string.IsNullOrWhiteSpace(request.Currency))
        {
            return Result<PlanResponse>.Failure(
                "Currency is required when price is greater than 0",
                errorCode: 400); // Bad Request
        }

        // ── Paso 7: Crear la entidad Plan con Plan.Create() ───────────
        // Plan.Create() valida todas las reglas de negocio
        var plan = Plan.Create(
            projectId: request.ProjectId,
            name: request.Name,
            slug: slug,
            price: request.Price,
            currency: request.Currency,
            billingCycle: billingCycle,
            isPublic: request.IsPublic);

        // ── Paso 8: Guardar en la base de datos ───────────────────────
        await _planRepository.AddAsync(plan, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ── Paso 9: Mapear a PlanResponse ────────────────────────────
        var response = new PlanResponse
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
            // Features se mapean de la colección de features del plan
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

        // ── Paso 10: Devolver el resultado ────────────────────────────
        return Result<PlanResponse>.Success(response);
    }

    /// <summary>
    /// Genera un slug automático desde el nombre del plan.
    /// 
    /// Conversiones:
    /// - "Pro Plan" → "pro-plan"
    /// - "Enterprise@2025!" → "enterprise2025"
    /// - Espacios múltiples se colapsan a un guión
    /// - Se eliminan caracteres especiales (solo letras, números, guiones)
    /// </summary>
    private static string GenerateSlugFromName(string name)
    {
        // Convertir a minúsculas
        var slug = name.ToLowerInvariant();

        // Reemplazar espacios con guiones
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");

        // Eliminar caracteres especiales (solo mantener letras, números, guiones)
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9-]", "");

        // Eliminar guiones consecutivos
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");

        // Eliminar guiones al inicio y final
        slug = slug.Trim('-');

        return slug;
    }

    /// <summary>
    /// Normaliza un slug proporcionado por el usuario.
    /// </summary>
    private static string NormalizeSlug(string slug)
    {
        var normalized = slug.ToLowerInvariant().Trim();
        
        // Aplicar las mismas reglas que GenerateSlugFromName
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"\s+", "-");
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"[^a-z0-9-]", "");
        normalized = System.Text.RegularExpressions.Regex.Replace(normalized, @"-+", "-");
        normalized = normalized.Trim('-');

        return normalized;
    }
}

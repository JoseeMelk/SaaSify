using MediatR;
using SaaSify.Application.Common;
using SaaSify.Application.Features.Projects.Commands;
using SaaSify.Application.Features.Projects.Dtos;
using SaaSify.Application.Interfaces;
using SaaSify.Domain.Interfaces;
using SaaSify.Domain.Entities;

namespace SaaSify.Application.Features.Projects.Commands.Handlers;

/// <summary>
/// Handler para CreateProjectCommand.
/// 
/// Se encarga de crear un nuevo proyecto para el developer autenticado.
/// La API key se genera aquí y se devuelve una sola vez en la response.
/// </summary>
public class CreateProjectCommandHandler : IRequestHandler<CreateProjectCommand, Result<CreateProjectResponse>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IUserRepository _userRepository;
    private readonly IApiKeyService _apiKeyService;
    private readonly IUnitOfWork _unitOfWork;

    public CreateProjectCommandHandler(
        IProjectRepository projectRepository,
        IUserRepository userRepository,
        IApiKeyService apiKeyService,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _userRepository = userRepository;
        _apiKeyService = apiKeyService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<CreateProjectResponse>> Handle(
        CreateProjectCommand request,
        CancellationToken cancellationToken)
    {
        // ── Paso 1: Verificar que el owner existe ──────────────────────────
        // Se confirma que el developer autenticado existe en la BD.
        // Aunque el JWT es válido, el usuario podría haber sido eliminado.
        var owner = await _userRepository.GetByIdAsync(request.OwnerId, cancellationToken);
 
        if (owner is null)
            return Result<CreateProjectResponse>.Failure(
                "User not found",
                errorCode: 404);
 
        // ── Paso 2: Generar o normalizar el slug ───────────────────────────
        // Si el developer no proveyó un slug, se genera desde el nombre.
        // "Mi SaaS App" → "mi-saas-app"
        var slug = string.IsNullOrWhiteSpace(request.Slug)
            ? GenerateSlug(request.Name)
            : request.Slug.Trim().ToLowerInvariant();
 
        // ── Paso 3: Verificar que el slug no esté ocupado ─────────────────
        // El slug es único globalmente — no puede repetirse entre proyectos.
        var slugExists = await _projectRepository.ExistsBySlugAsync(slug, cancellationToken);
 
        if (slugExists)
            return Result<CreateProjectResponse>.Failure(
                $"The slug '{slug}' is already in use. Please choose a different name or provide a unique slug",
                errorCode: 409);
 
        // ── Paso 4: Generar la API key ─────────────────────────────────────
        // Se generan los tres componentes: rawKey, hash y prefix.
        // Solo rawKey se devuelve al developer — hash y prefix se guardan en BD.
        var apiKey = _apiKeyService.Generate();
 
        // ── Paso 5: Crear la entidad Project en el dominio ─────────────────
        // Se usa el factory method del dominio para garantizar un objeto válido.
        var project = Project.Create(
            ownerId: request.OwnerId,
            name: request.Name.Trim(),
            slug: slug,
            apiKeyHash: apiKey.Hash,
            apiKeyPrefix: apiKey.Prefix);
 
        // ── Paso 6: Guardar en la base de datos ────────────────────────────
        await _projectRepository.AddAsync(project, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);
 
        // ── Paso 7: Devolver el proyecto con la API key completa ───────────
        // apiKey.RawKey se devuelve SOLO AQUÍ — nunca más en ningún endpoint.
        var response = new CreateProjectResponse
        {
            Id = project.Id,
            Name = project.Name,
            Slug = project.Slug,
            ApiKey = apiKey.RawKey,
            Status = project.Status.ToString(),
            CreatedAt = project.CreatedAt
        };
 
        return Result<CreateProjectResponse>.Success(response);
    }
 
    /// <summary>
    /// Se genera un slug URL-friendly desde el nombre del proyecto.
    /// 
    /// Pasos:
    /// 1. Se convierte a minúsculas
    /// 2. Se reemplazan espacios por guiones
    /// 3. Se eliminan caracteres especiales
    /// 4. Se eliminan guiones duplicados
    /// 
    /// Ejemplo: "Mi SaaS App v2!" → "mi-saas-app-v2"
    /// </summary>
    private static string GenerateSlug(string name)
    {
        var slug = name.Trim().ToLowerInvariant();
 
        // Se reemplazan espacios y caracteres no alfanuméricos por guiones.
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"\s+", "-");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-+", "-");
 
        // Se eliminan guiones al inicio o final.
        return slug.Trim('-');
    }
}
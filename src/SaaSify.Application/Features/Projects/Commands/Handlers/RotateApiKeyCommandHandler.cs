using MediatR;
using SaaSify.Application.Common;
using SaaSify.Application.Features.Projects.Commands;
using SaaSify.Application.Features.Projects.Dtos;
using SaaSify.Application.Interfaces;
using SaaSify.Domain.Interfaces;

namespace SaaSify.Application.Features.Projects.Commands.Handlers;

/// <summary>
/// Handler para RotateApiKeyCommand.
/// 
/// Se genera una nueva API key para el proyecto.
/// La key anterior queda invalidada inmediatamente.
/// La nueva key se devuelve una sola vez en la response.
/// </summary>
public class RotateApiKeyCommandHandler
    : IRequestHandler<RotateApiKeyCommand, Result<RotateApiKeyResponse>>
{
    private readonly IProjectRepository _projectRepository;
    private readonly IApiKeyService _apiKeyService;
    private readonly IUnitOfWork _unitOfWork;

    public RotateApiKeyCommandHandler(
        IProjectRepository projectRepository,
        IApiKeyService apiKeyService,
        IUnitOfWork unitOfWork)
    {
        _projectRepository = projectRepository;
        _apiKeyService = apiKeyService;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<RotateApiKeyResponse>> Handle(
        RotateApiKeyCommand request,
        CancellationToken cancellationToken)
    {
        // ── Paso 1: Verificar que el proyecto existe ───────────────────────
        var project = await _projectRepository.GetByIdAsync(
            request.ProjectId,
            cancellationToken);

        if (project is null)
            return Result<RotateApiKeyResponse>.Failure(
                "Project not found",
                errorCode: 404);

        // ── Paso 2: Verificar que quien rota es el owner ───────────────────
        // Se evita que un developer rote la key de un proyecto ajeno.
        if (project.OwnerId != request.OwnerId)
            return Result<RotateApiKeyResponse>.Failure(
                "You do not have permission to modify this project",
                errorCode: 403);

        // ── Paso 3: Generar nueva API key ──────────────────────────────────
        var newApiKey = _apiKeyService.Generate();

        // ── Paso 4: Actualizar el proyecto con la nueva key ────────────────
        // Se usa el método del dominio — el proyecto controla su propio estado.
        project.RotateApiKey(newApiKey.Hash, newApiKey.Prefix);

        // ── Paso 5: Guardar en la base de datos ────────────────────────────
        _projectRepository.Update(project);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        // ── Paso 6: Devolver la nueva key completa ─────────────────────────
        // newApiKey.RawKey se devuelve SOLO AQUÍ — nunca más.
        var response = new RotateApiKeyResponse
        {
            ApiKey = newApiKey.RawKey,
            RotatedAt = DateTime.UtcNow
        };

        return Result<RotateApiKeyResponse>.Success(response);
    }
}
using MediatR;
using SaaSify.Application.Common;
using SaaSify.Application.Features.Projects.Dtos;
using SaaSify.Application.Features.Projects.Queries;
using SaaSify.Domain.Interfaces;

namespace SaaSify.Application.Features.Projects.Queries.Handlers;

/// <summary>
/// Handler para GetProjectByIdQuery.
/// 
/// Se obtiene un proyecto por su ID.
/// Se verifica que pertenece al developer autenticado.
/// La API key nunca se devuelve en consultas.
/// </summary>
public class GetProjectByIdQueryHandler
    : IRequestHandler<GetProjectByIdQuery, Result<ProjectResponse>>
{
    private readonly IProjectRepository _projectRepository;

    public GetProjectByIdQueryHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<Result<ProjectResponse>> Handle(
        GetProjectByIdQuery request,
        CancellationToken cancellationToken)
    {
        // ── Paso 1: Buscar el proyecto por ID ──────────────────────────────
        var project = await _projectRepository.GetByIdAsync(
            request.ProjectId,
            cancellationToken);

        if (project is null)
            return Result<ProjectResponse>.Failure(
                "Project not found",
                errorCode: 404);

        // ── Paso 2: Verificar que pertenece al owner autenticado ───────────
        // Si el proyecto existe pero no le pertenece, se devuelve 404.
        // Se usa 404 en vez de 403 para no revelar que el proyecto existe.
        if (project.OwnerId != request.OwnerId)
            return Result<ProjectResponse>.Failure(
                "Project not found",
                errorCode: 404);

        // ── Paso 3: Mapear a response ──────────────────────────────────────
        var response = new ProjectResponse
        {
            Id = project.Id,
            Name = project.Name,
            Slug = project.Slug,
            Status = project.Status.ToString(),
            CreatedAt = project.CreatedAt,
            UpdatedAt = project.UpdatedAt
        };

        return Result<ProjectResponse>.Success(response);
    }
}
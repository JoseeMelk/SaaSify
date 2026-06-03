using MediatR;
using SaaSify.Application.Common;
using SaaSify.Application.Features.Projects.Dtos;
using SaaSify.Application.Features.Projects.Queries;
using SaaSify.Domain.Interfaces;

namespace SaaSify.Application.Features.Projects.Queries.Handlers;

/// <summary>
/// Handler para ListProjectsQuery.
/// 
/// Se obtienen todos los proyectos del developer autenticado.
/// El repositorio ya filtra por OwnerId — nunca devuelve proyectos ajenos.
/// </summary>
public class ListProjectsQueryHandler
    : IRequestHandler<ListProjectsQuery, Result<IReadOnlyList<ProjectResponse>>>
{
    private readonly IProjectRepository _projectRepository;

    public ListProjectsQueryHandler(IProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
    }

    public async Task<Result<IReadOnlyList<ProjectResponse>>> Handle(
        ListProjectsQuery request,
        CancellationToken cancellationToken)
    {
        // ── Paso 1: Obtener proyectos del owner ────────────────────────────
        // El repositorio filtra por OwnerId — ya está ordenado por creación descendente.
        var projects = await _projectRepository.GetByOwnerIdAsync(
            request.OwnerId,
            cancellationToken);

        // ── Paso 2: Mapear a response ──────────────────────────────────────
        // Se mapea cada proyecto a su DTO — sin ApiKey ni información sensible.
        var response = projects.Select(p => new ProjectResponse
        {
            Id = p.Id,
            Name = p.Name,
            Slug = p.Slug,
            Status = p.Status.ToString(),
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        }).ToList();

        return Result<IReadOnlyList<ProjectResponse>>.Success(response);
    }
}
using SaaSify.Domain.Entities;

namespace SaaSify.Domain.Interfaces;

public interface IPlanRepository : IRepository<Plan>
{
    // Buscar por slug dentro de un proyecto — el dev usa "pro", "enterprise" en su código
    Task<Plan?> GetBySlugAsync(Guid projectId, string slug, CancellationToken cancellationToken = default);

    // Listar todos los planes de un proyecto — incluye features
    Task<IReadOnlyList<Plan>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);

    // Verificar si el slug ya existe en el proyecto
    Task<bool> ExistsBySlugAsync(Guid projectId, string slug, CancellationToken cancellationToken = default);
}
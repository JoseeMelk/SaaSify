using SaaSify.Domain.Entities;

namespace SaaSify.Domain.Interfaces;

public interface IProjectRepository : IRepository<Project>
{
    // Buscar por slug — usado al crear proyectos para verificar unicidad
    Task<Project?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default);

    // Buscar por prefijo de API key — primer paso del lookup de autenticación
    Task<Project?> GetByApiKeyPrefixAsync(string prefix, CancellationToken cancellationToken = default);

    // Listar todos los proyectos de un usuario
    Task<IReadOnlyList<Project>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default);

    // Verificar si un slug ya existe — para dar feedback antes de intentar crear
    Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default);
}
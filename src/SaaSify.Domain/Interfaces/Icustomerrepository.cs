using SaaSify.Domain.Entities;

namespace SaaSify.Domain.Interfaces;

public interface ICustomerRepository : IRepository<Customer>
{
    // La búsqueda principal — el dev siempre busca por su propio ID de usuario
    Task<Customer?> GetByExternalIdAsync(Guid projectId, string externalId, CancellationToken cancellationToken = default);

    // Listar customers de un proyecto — para el dashboard del dev
    Task<IReadOnlyList<Customer>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);

    // Verificar si ya existe un customer con ese externalId en el proyecto
    Task<bool> ExistsByExternalIdAsync(Guid projectId, string externalId, CancellationToken cancellationToken = default);
}
using SaaSify.Domain.Entities;

namespace SaaSify.Domain.Interfaces;

public interface IPlanRepository : IRepository<Plan>
{
    Task<Plan?> GetBySlugAsync(Guid projectId, string slug, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Plan>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default);
    Task<bool> ExistsBySlugAsync(Guid projectId, string slug, CancellationToken cancellationToken = default);

    // Igual que GetByIdAsync pero incluye las features del plan.
    // Se usa en el entitlement check — sin este Include, plan.Features llega vacío.
    Task<Plan?> GetByIdWithFeaturesAsync(Guid id, CancellationToken cancellationToken = default);
}
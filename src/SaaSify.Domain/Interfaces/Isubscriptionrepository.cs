using SaaSify.Domain.Entities;

namespace SaaSify.Domain.Interfaces;

public interface ISubscriptionRepository : IRepository<Subscription>
{
    // La suscripción activa actual de un customer — la más consultada del sistema
    Task<Subscription?> GetActiveByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    // Historial completo de suscripciones de un customer
    Task<IReadOnlyList<Subscription>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default);

    // Para el background job — detecta suscripciones cuyo período venció
    // y aún no están marcadas como Expired o Cancelled
    Task<IReadOnlyList<Subscription>> GetExpiredAsync(DateTime asOf, CancellationToken cancellationToken = default);
}
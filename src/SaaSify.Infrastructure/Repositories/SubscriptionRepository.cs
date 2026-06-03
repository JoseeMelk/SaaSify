using Microsoft.EntityFrameworkCore;
using SaaSify.Domain.Entities;
using SaaSify.Domain.Enums;
using SaaSify.Domain.Interfaces;
using SaaSify.Infrastructure.Persistence;

namespace SaaSify.Infrastructure.Repositories;

public class SubscriptionRepository : Repository<Subscription>, ISubscriptionRepository
{
    public SubscriptionRepository(AppDbContext context) : base(context) { }

    public async Task<Subscription?> GetActiveByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .FirstOrDefaultAsync(
                s => s.CustomerId == customerId && s.Status == SubscriptionStatus.Active,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Subscription>> GetByCustomerIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        return await _context.Subscriptions
            .Where(s => s.CustomerId == customerId)
            .OrderByDescending(s => s.StartedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Subscription>> GetExpiredAsync(DateTime asOf, CancellationToken cancellationToken = default)
    {
        // Suscripciones cuyo período venció pero no están marcadas como Expired o Cancelled
        return await _context.Subscriptions
            .Where(s => s.CurrentPeriodEnd < asOf 
                     && s.Status != SubscriptionStatus.Expired 
                     && s.Status != SubscriptionStatus.Cancelled)
            .ToListAsync(cancellationToken);
    }
}

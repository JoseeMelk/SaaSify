using Microsoft.EntityFrameworkCore;
using SaaSify.Domain.Entities;
using SaaSify.Domain.Interfaces;
using SaaSify.Infrastructure.Persistence;

namespace SaaSify.Infrastructure.Repositories;

public class CustomerRepository : Repository<Customer>, ICustomerRepository
{
    public CustomerRepository(AppDbContext context) : base(context) { }

    public async Task<Customer?> GetByExternalIdAsync(Guid projectId, string externalId, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .Include(c => c.Subscriptions)
            .FirstOrDefaultAsync(
                c => c.ProjectId == projectId && c.ExternalId == externalId,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Customer>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .Where(c => c.ProjectId == projectId)
            .Include(c => c.Subscriptions)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsByExternalIdAsync(Guid projectId, string externalId, CancellationToken cancellationToken = default)
    {
        return await _context.Customers
            .AnyAsync(c => c.ProjectId == projectId && c.ExternalId == externalId, cancellationToken);
    }
}

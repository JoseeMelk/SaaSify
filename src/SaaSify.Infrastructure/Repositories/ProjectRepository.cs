using Microsoft.EntityFrameworkCore;
using SaaSify.Domain.Entities;
using SaaSify.Domain.Interfaces;
using SaaSify.Infrastructure.Persistence;

namespace SaaSify.Infrastructure.Repositories;

public class ProjectRepository : Repository<Project>, IProjectRepository
{
    public ProjectRepository(AppDbContext context) : base(context) { }

    public async Task<Project?> GetBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();

        return await _context.Projects
            .Include(p => p.Plans)
                .ThenInclude(pl => pl.Features)
            .FirstOrDefaultAsync(p => p.Slug == normalizedSlug, cancellationToken);
    }

    public async Task<Project?> GetByApiKeyPrefixAsync(string prefix, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .FirstOrDefaultAsync(p => p.ApiKeyPrefix == prefix, cancellationToken);
    }

    public async Task<IReadOnlyList<Project>> GetByOwnerIdAsync(Guid ownerId, CancellationToken cancellationToken = default)
    {
        return await _context.Projects
            .Where(p => p.OwnerId == ownerId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsBySlugAsync(string slug, CancellationToken cancellationToken = default)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();

        return await _context.Projects
            .AnyAsync(p => p.Slug == normalizedSlug, cancellationToken);
    }
}

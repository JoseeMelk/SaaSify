using Microsoft.EntityFrameworkCore;
using SaaSify.Domain.Entities;
using SaaSify.Domain.Interfaces;
using SaaSify.Infrastructure.Persistence;

namespace SaaSify.Infrastructure.Repositories;

public class PlanRepository : Repository<Plan>, IPlanRepository
{
    public PlanRepository(AppDbContext context) : base(context) { }

    public async Task<Plan?> GetBySlugAsync(Guid projectId, string slug, CancellationToken cancellationToken = default)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();

        return await _context.Plans
            .Include(p => p.Features)
            .FirstOrDefaultAsync(
                p => p.ProjectId == projectId && p.Slug == normalizedSlug,
                cancellationToken);
    }

    public async Task<IReadOnlyList<Plan>> GetByProjectIdAsync(Guid projectId, CancellationToken cancellationToken = default)
    {
        return await _context.Plans
            .Where(p => p.ProjectId == projectId)
            .Include(p => p.Features)
            .OrderBy(p => p.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<bool> ExistsBySlugAsync(Guid projectId, string slug, CancellationToken cancellationToken = default)
    {
        var normalizedSlug = slug.Trim().ToLowerInvariant();

        return await _context.Plans
            .AnyAsync(p => p.ProjectId == projectId && p.Slug == normalizedSlug, cancellationToken);
    }

    // Se incluyen las features para que plan.HasFeature() funcione correctamente.
    // Sin el Include, EF Core devuelve el plan con Features vacío.
    public async Task<Plan?> GetByIdWithFeaturesAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Plans
            .Include(p => p.Features)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
    }
}
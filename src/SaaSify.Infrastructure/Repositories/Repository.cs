using Microsoft.EntityFrameworkCore;
using SaaSify.Domain.Common;
using SaaSify.Domain.Interfaces;
using SaaSify.Infrastructure.Persistence;

namespace SaaSify.Infrastructure.Repositories;

/// <summary>
/// Repositorio genérico base que implementa las operaciones CRUD comunes.
/// Todas las entidades heredan de aquí.
/// </summary>
public class Repository<T> : IRepository<T> where T : Entity
{
    protected readonly AppDbContext _context;

    public Repository(AppDbContext context)
    {
        _context = context;
    }

    public virtual async Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>()
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);
    }

    public virtual async Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        await _context.Set<T>().AddAsync(entity, cancellationToken);
    }

    public virtual void Update(T entity)
    {
        _context.Set<T>().Update(entity);
    }

    public virtual void Delete(T entity)
    {
        _context.Set<T>().Remove(entity);
    }
}

namespace SaaSify.Domain.Interfaces;

// Unit of Work — agrupa todas las operaciones en una sola transacción
// Ningún repositorio guarda en DB por sí solo — todos esperan a SaveChangesAsync
// Esto garantiza que si algo falla a mitad de una operación, nada queda a medias
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
namespace SaaSify.Domain.Interfaces;

// T debe ser una entidad del dominio
// Esta interfaz define las operaciones comunes a todos los repositorios
public interface IRepository<T> where T : Common.Entity
{
    // Buscar por ID — devuelve null si no existe (no lanza excepción)
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    // Agregar al contexto — aún no guarda en DB (eso lo hace UnitOfWork)
    Task AddAsync(T entity, CancellationToken cancellationToken = default);

    // Marcar como modificado
    void Update(T entity);

    // Marcar como eliminado (soft delete lo maneja EF Core automáticamente)
    void Delete(T entity);
}
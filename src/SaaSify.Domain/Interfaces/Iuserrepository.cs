using SaaSify.Domain.Entities;

namespace SaaSify.Domain.Interfaces;

public interface IUserRepository : IRepository<User>
{
    // Buscar por email — usado en login y para verificar duplicados al registrar
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);

    // Verificar si un email ya existe — más eficiente que traer el objeto completo
    Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default);
}
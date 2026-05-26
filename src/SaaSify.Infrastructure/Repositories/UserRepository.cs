using Microsoft.EntityFrameworkCore;
using SaaSify.Domain.Entities;
using SaaSify.Domain.Interfaces;
using SaaSify.Infrastructure.Persistence;

namespace SaaSify.Infrastructure.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(AppDbContext context) : base(context) { }

    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return await _context.Users
            .FirstOrDefaultAsync(u => u.Email == normalizedEmail, cancellationToken);
    }

    public async Task<bool> ExistsByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        var normalizedEmail = email.Trim().ToLowerInvariant();

        return await _context.Users
            .AnyAsync(u => u.Email == normalizedEmail, cancellationToken);
    }
}

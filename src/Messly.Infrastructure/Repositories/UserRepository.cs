using Messly.Application.Interfaces.Persistence;
using Messly.Domain.Entities;
using Messly.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Messly.Infrastructure.Repositories;

public class UserRepository(MesslyDbContext context) : Repository<User>(context), IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        => await DbSet.FirstOrDefaultAsync(u => u.Email == email, cancellationToken);
}

using Messly.Domain.Entities;

namespace Messly.Application.Interfaces.Persistence;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
}

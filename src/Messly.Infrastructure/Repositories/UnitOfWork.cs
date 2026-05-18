using Messly.Application.Interfaces.Persistence;
using Messly.Infrastructure.Data;

namespace Messly.Infrastructure.Repositories;

public class UnitOfWork(MesslyDbContext context) : IUnitOfWork
{
    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => context.SaveChangesAsync(cancellationToken);
}

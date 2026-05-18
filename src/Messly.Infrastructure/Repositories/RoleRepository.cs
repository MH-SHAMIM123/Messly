using Messly.Application.Interfaces.Persistence;
using Messly.Domain.Entities;
using Messly.Domain.Enums;
using Messly.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Messly.Infrastructure.Repositories;

public class RoleRepository(MesslyDbContext context) : Repository<Role>(context), IRoleRepository
{
    public async Task<Role?> GetByRoleTypeAsync(RoleType roleType, CancellationToken cancellationToken = default)
        => await DbSet.FirstOrDefaultAsync(r => r.RoleType == roleType, cancellationToken);
}

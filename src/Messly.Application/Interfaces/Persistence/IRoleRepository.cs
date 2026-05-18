using Messly.Domain.Entities;
using Messly.Domain.Enums;

namespace Messly.Application.Interfaces.Persistence;

public interface IRoleRepository : IRepository<Role>
{
    Task<Role?> GetByRoleTypeAsync(RoleType roleType, CancellationToken cancellationToken = default);
}

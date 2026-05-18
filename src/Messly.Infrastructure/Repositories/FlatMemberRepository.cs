using Messly.Application.Interfaces.Persistence;
using Messly.Domain.Entities;
using Messly.Domain.Enums;
using Messly.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Messly.Infrastructure.Repositories;

public class FlatMemberRepository(MesslyDbContext context)
    : Repository<FlatMember>(context), IFlatMemberRepository
{
    public async Task<IReadOnlyList<FlatMember>> GetByFlatIdAsync(Guid flatId, CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Include(m => m.User)
            .Include(m => m.Role)
            .Where(m => m.FlatId == flatId)
            .OrderBy(m => m.User!.FullName)
            .ToListAsync(cancellationToken);

    public async Task<FlatMember?> GetByIdWithDetailsAsync(Guid id, Guid flatId, CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Include(m => m.User)
            .Include(m => m.Role)
            .FirstOrDefaultAsync(m => m.Id == id && m.FlatId == flatId, cancellationToken);

    public async Task<FlatMember?> GetByIdWithDetailsForUpdateAsync(Guid id, Guid flatId, CancellationToken cancellationToken = default)
        => await DbSet
            .Include(m => m.User)
            .Include(m => m.Role)
            .FirstOrDefaultAsync(m => m.Id == id && m.FlatId == flatId, cancellationToken);

    public async Task<bool> IsActiveMemberOfFlatAsync(Guid flatId, Guid userId, CancellationToken cancellationToken = default)
        => await DbSet.AnyAsync(
            m => m.FlatId == flatId && m.UserId == userId && m.IsActive,
            cancellationToken);

    public async Task<int> CountActiveManagersAsync(Guid flatId, CancellationToken cancellationToken = default)
        => await DbSet
            .Include(m => m.Role)
            .CountAsync(
                m => m.FlatId == flatId && m.IsActive && m.Role!.RoleType == RoleType.Manager,
                cancellationToken);
}

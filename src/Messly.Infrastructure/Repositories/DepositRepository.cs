using Messly.Application.Interfaces.Persistence;
using Messly.Domain.Entities;
using Messly.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Messly.Infrastructure.Repositories;

public class DepositRepository(MesslyDbContext context) : Repository<Deposit>(context), IDepositRepository
{
    public override async Task<Deposit?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == id, cancellationToken);

    public async Task<Deposit?> GetByIdAndFlatAsync(Guid id, Guid flatId, CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == id && d.FlatId == flatId, cancellationToken);

    public async Task<Deposit?> GetByIdForUpdateAndFlatAsync(Guid id, Guid flatId, CancellationToken cancellationToken = default)
        => await DbSet
            .Include(d => d.User)
            .FirstOrDefaultAsync(d => d.Id == id && d.FlatId == flatId, cancellationToken);

    public async Task<IReadOnlyList<Deposit>> GetByFlatIdAsync(Guid flatId, CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Include(d => d.User)
            .Where(d => d.FlatId == flatId)
            .OrderByDescending(d => d.DepositDate)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Deposit>> GetByFlatAndMonthAsync(
        Guid flatId,
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        var start = new DateOnly(year, month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        return await DbSet
            .AsNoTracking()
            .Where(d => d.FlatId == flatId && d.DepositDate >= start && d.DepositDate <= end)
            .ToListAsync(cancellationToken);
    }

    public async Task<decimal> GetTotalByFlatAndMonthAsync(Guid flatId, int year, int month, CancellationToken cancellationToken = default)
    {
        var start = new DateOnly(year, month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        return await DbSet
            .Where(d => d.FlatId == flatId && d.DepositDate >= start && d.DepositDate <= end)
            .SumAsync(d => d.Amount, cancellationToken);
    }
}

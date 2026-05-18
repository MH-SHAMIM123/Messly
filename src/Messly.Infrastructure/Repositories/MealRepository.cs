using Messly.Application.Interfaces.Persistence;
using Messly.Domain.Entities;
using Messly.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Messly.Infrastructure.Repositories;

public class MealRepository(MesslyDbContext context) : Repository<Meal>(context), IMealRepository
{
    public async Task<Meal?> GetByIdAndFlatAsync(Guid id, Guid flatId, CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Include(m => m.User)
            .FirstOrDefaultAsync(m => m.Id == id && m.FlatId == flatId, cancellationToken);

    public async Task<IReadOnlyList<Meal>> GetByFlatAndDateAsync(Guid flatId, DateOnly date, CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Include(m => m.User)
            .Where(m => m.FlatId == flatId && m.MealDate == date)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Meal>> GetByFlatAndDateForUpdateAsync(Guid flatId, DateOnly date, CancellationToken cancellationToken = default)
        => await DbSet
            .Where(m => m.FlatId == flatId && m.MealDate == date)
            .ToListAsync(cancellationToken);

    public async Task<IReadOnlyList<Meal>> GetByFlatAndMonthAsync(Guid flatId, int year, int month, CancellationToken cancellationToken = default)
    {
        var start = new DateOnly(year, month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        return await DbSet
            .AsNoTracking()
            .Include(m => m.User)
            .Where(m => m.FlatId == flatId && m.MealDate >= start && m.MealDate <= end)
            .ToListAsync(cancellationToken);
    }
}

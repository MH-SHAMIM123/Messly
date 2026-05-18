using Messly.Application.Interfaces.Persistence;
using Messly.Domain.Entities;
using Messly.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Messly.Infrastructure.Repositories;

public class ExpenseRepository(MesslyDbContext context) : Repository<Expense>(context), IExpenseRepository
{
    public override async Task<Expense?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Include(e => e.Category)
            .Include(e => e.PaidBy)
            .FirstOrDefaultAsync(e => e.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Expense>> GetByFlatIdAsync(Guid flatId, CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Include(e => e.Category)
            .Include(e => e.PaidBy)
            .Where(e => e.FlatId == flatId)
            .OrderByDescending(e => e.ExpenseDate)
            .ToListAsync(cancellationToken);

    public async Task<decimal> GetTotalByFlatAndMonthAsync(Guid flatId, int year, int month, CancellationToken cancellationToken = default)
    {
        var start = new DateOnly(year, month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        return await DbSet
            .Where(e => e.FlatId == flatId && e.ExpenseDate >= start && e.ExpenseDate <= end)
            .SumAsync(e => e.Amount, cancellationToken);
    }
}

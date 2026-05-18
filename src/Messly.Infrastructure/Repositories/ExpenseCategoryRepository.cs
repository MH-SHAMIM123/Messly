using Messly.Application.Interfaces.Persistence;
using Messly.Domain.Entities;
using Messly.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Messly.Infrastructure.Repositories;

public class ExpenseCategoryRepository(MesslyDbContext context)
    : Repository<ExpenseCategory>(context), IExpenseCategoryRepository
{
    public async Task<IReadOnlyList<ExpenseCategory>> GetByFlatIdAsync(Guid flatId, CancellationToken cancellationToken = default)
        => await DbSet
            .AsNoTracking()
            .Where(c => c.FlatId == flatId)
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
}

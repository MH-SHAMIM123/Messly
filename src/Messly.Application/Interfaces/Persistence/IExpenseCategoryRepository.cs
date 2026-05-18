using Messly.Domain.Entities;

namespace Messly.Application.Interfaces.Persistence;

public interface IExpenseCategoryRepository : IRepository<ExpenseCategory>
{
    Task<IReadOnlyList<ExpenseCategory>> GetByFlatIdAsync(Guid flatId, CancellationToken cancellationToken = default);
}

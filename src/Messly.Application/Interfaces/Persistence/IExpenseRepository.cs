using Messly.Domain.Entities;

namespace Messly.Application.Interfaces.Persistence;

public interface IExpenseRepository : IRepository<Expense>
{
    Task<IReadOnlyList<Expense>> GetByFlatIdAsync(Guid flatId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalByFlatAndMonthAsync(Guid flatId, int year, int month, CancellationToken cancellationToken = default);
}

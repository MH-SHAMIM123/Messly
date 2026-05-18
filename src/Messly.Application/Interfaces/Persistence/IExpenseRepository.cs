using Messly.Domain.Entities;

namespace Messly.Application.Interfaces.Persistence;

public interface IExpenseRepository : IRepository<Expense>
{
    Task<Expense?> GetByIdAndFlatAsync(Guid id, Guid flatId, CancellationToken cancellationToken = default);
    Task<Expense?> GetByIdForUpdateAndFlatAsync(Guid id, Guid flatId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Expense>> GetByFlatIdAsync(Guid flatId, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalByFlatAndMonthAsync(Guid flatId, int year, int month, CancellationToken cancellationToken = default);
}

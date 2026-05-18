using Messly.Domain.Entities;

namespace Messly.Application.Interfaces.Persistence;

public interface IDepositRepository : IRepository<Deposit>
{
    Task<Deposit?> GetByIdAndFlatAsync(Guid id, Guid flatId, CancellationToken cancellationToken = default);
    Task<Deposit?> GetByIdForUpdateAndFlatAsync(Guid id, Guid flatId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Deposit>> GetByFlatIdAsync(Guid flatId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Deposit>> GetByFlatAndMonthAsync(Guid flatId, int year, int month, CancellationToken cancellationToken = default);
    Task<decimal> GetTotalByFlatAndMonthAsync(Guid flatId, int year, int month, CancellationToken cancellationToken = default);
}

using Messly.Domain.Entities;

namespace Messly.Application.Interfaces.Persistence;

public interface IMealRepository : IRepository<Meal>
{
    Task<Meal?> GetByIdAndFlatAsync(Guid id, Guid flatId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Meal>> GetByFlatAndDateAsync(Guid flatId, DateOnly date, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Meal>> GetByFlatAndDateForUpdateAsync(Guid flatId, DateOnly date, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Meal>> GetByFlatAndMonthAsync(Guid flatId, int year, int month, CancellationToken cancellationToken = default);
}

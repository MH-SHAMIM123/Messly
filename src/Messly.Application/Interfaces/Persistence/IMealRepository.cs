using Messly.Domain.Entities;

namespace Messly.Application.Interfaces.Persistence;

public interface IMealRepository : IRepository<Meal>
{
    Task<IReadOnlyList<Meal>> GetByFlatAndDateAsync(Guid flatId, DateOnly date, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<Meal>> GetByFlatAndMonthAsync(Guid flatId, int year, int month, CancellationToken cancellationToken = default);
}

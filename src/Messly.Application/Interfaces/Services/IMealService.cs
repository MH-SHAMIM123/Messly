using Messly.Application.DTOs;

namespace Messly.Application.Interfaces.Services;

public interface IMealService
{
    Task<IReadOnlyList<MealEntryDto>> GetMealEntriesByDateAsync(Guid flatId, DateOnly date, CancellationToken cancellationToken = default);
    Task SaveDailyEntriesAsync(Guid flatId, DateOnly date, IReadOnlyList<MealEntryDto> entries, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MealSummaryDto>> GetMealTotalsByMonthAsync(Guid flatId, int year, int month, CancellationToken cancellationToken = default);
}

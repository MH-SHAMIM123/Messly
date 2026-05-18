using Messly.Application.DTOs;

namespace Messly.Application.Interfaces.Services;

public interface IMealService
{
    Task<IReadOnlyList<MealEntryDto>> GetMealEntriesByDateAsync(DateOnly date, CancellationToken cancellationToken = default);
    Task SaveDailyEntriesAsync(DateOnly date, IReadOnlyList<MealEntryDto> entries, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MealSummaryDto>> GetMealTotalsByMonthAsync(int year, int month, CancellationToken cancellationToken = default);
}

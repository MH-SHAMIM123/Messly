using Messly.Application.Common;
using Messly.Application.DTOs;
using Messly.Application.Interfaces.Persistence;
using Messly.Application.Interfaces.Services;
using Messly.Domain.Entities;

namespace Messly.Application.Services;

public class MealService(
    IMealRepository mealRepository,
    IFlatMemberRepository memberRepository,
    IUnitOfWork unitOfWork) : IMealService
{
    public async Task<IReadOnlyList<MealEntryDto>> GetDailyEntriesAsync(Guid flatId, DateOnly date, CancellationToken cancellationToken = default)
    {
        var members = await memberRepository.GetByFlatIdAsync(flatId, cancellationToken);
        var meals = await mealRepository.GetByFlatAndDateAsync(flatId, date, cancellationToken);

        return members
            .Where(m => m.IsActive)
            .Select(m =>
            {
                var meal = meals.FirstOrDefault(x => x.UserId == m.UserId);
                return new MealEntryDto
                {
                    UserId = m.UserId,
                    MemberName = m.User?.FullName ?? string.Empty,
                    BreakfastCount = meal?.BreakfastCount ?? 0,
                    LunchCount = meal?.LunchCount ?? 0,
                    DinnerCount = meal?.DinnerCount ?? 0
                };
            })
            .ToList();
    }

    public async Task SaveDailyEntriesAsync(Guid flatId, DateOnly date, IReadOnlyList<MealEntryDto> entries, CancellationToken cancellationToken = default)
    {
        foreach (var entry in entries)
        {
            if (entry.BreakfastCount is < 0 or > 3 ||
                entry.LunchCount is < 0 or > 3 ||
                entry.DinnerCount is < 0 or > 3)
            {
                throw new BusinessException($"Invalid meal counts for {entry.MemberName}. Each must be 0–3.");
            }

            var existing = (await mealRepository.GetByFlatAndDateAsync(flatId, date, cancellationToken))
                .FirstOrDefault(m => m.UserId == entry.UserId);

            if (existing is not null)
            {
                var tracked = await mealRepository.GetByIdForUpdateAsync(existing.Id, cancellationToken);
                if (tracked is null) continue;

                tracked.BreakfastCount = entry.BreakfastCount;
                tracked.LunchCount = entry.LunchCount;
                tracked.DinnerCount = entry.DinnerCount;
                tracked.UpdatedAt = DateTime.UtcNow;
                mealRepository.Update(tracked);
            }
            else if (entry.BreakfastCount + entry.LunchCount + entry.DinnerCount > 0)
            {
                await mealRepository.AddAsync(new Meal
                {
                    FlatId = flatId,
                    UserId = entry.UserId,
                    MealDate = date,
                    BreakfastCount = entry.BreakfastCount,
                    LunchCount = entry.LunchCount,
                    DinnerCount = entry.DinnerCount
                }, cancellationToken);
            }
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<MealSummaryDto>> GetMealTotalsByMonthAsync(Guid flatId, int year, int month, CancellationToken cancellationToken = default)
    {
        var meals = await mealRepository.GetByFlatAndMonthAsync(flatId, year, month, cancellationToken);

        return meals
            .GroupBy(m => m.UserId)
            .Select(g => new MealSummaryDto
            {
                UserId = g.Key,
                MemberName = g.First().User?.FullName ?? string.Empty,
                TotalBreakfast = g.Sum(x => x.BreakfastCount),
                TotalLunch = g.Sum(x => x.LunchCount),
                TotalDinner = g.Sum(x => x.DinnerCount),
                GrandTotal = g.Sum(x => x.TotalMealCount)
            })
            .OrderBy(s => s.MemberName)
            .ToList();
    }
}

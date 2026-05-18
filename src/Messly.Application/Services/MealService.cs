using FluentValidation;
using Messly.Application.Common;
using Messly.Application.DTOs;
using Messly.Application.Interfaces.Persistence;
using Messly.Application.Interfaces.Services;
using Messly.Domain.Entities;

namespace Messly.Application.Services;

public class MealService(
    IMealRepository mealRepository,
    IFlatMemberRepository memberRepository,
    IUnitOfWork unitOfWork,
    IValidator<MealEntryDto> entryValidator) : IMealService
{
    public async Task<IReadOnlyList<MealEntryDto>> GetMealEntriesByDateAsync(
        Guid flatId,
        DateOnly date,
        CancellationToken cancellationToken = default)
    {
        var members = await memberRepository.GetByFlatIdAsync(flatId, cancellationToken);
        var meals = await mealRepository.GetByFlatAndDateAsync(flatId, date, cancellationToken);
        var mealsByUser = meals.ToDictionary(m => m.UserId);

        return members
            .Where(m => m.IsActive)
            .Select(m =>
            {
                mealsByUser.TryGetValue(m.UserId, out var meal);
                return new MealEntryDto
                {
                    UserId = m.UserId,
                    MemberName = m.User?.FullName ?? string.Empty,
                    BreakfastCount = meal?.BreakfastCount ?? 0,
                    LunchCount = meal?.LunchCount ?? 0,
                    DinnerCount = meal?.DinnerCount ?? 0
                };
            })
            .OrderBy(e => e.MemberName)
            .ToList();
    }

    public async Task SaveDailyEntriesAsync(
        Guid flatId,
        DateOnly date,
        IReadOnlyList<MealEntryDto> entries,
        CancellationToken cancellationToken = default)
    {
        if (entries.Count == 0)
            throw new BusinessException("At least one meal entry is required.");

        foreach (var entry in entries)
            await ValidateEntryAsync(entry, cancellationToken);

        var existingByUser = (await mealRepository.GetByFlatAndDateForUpdateAsync(flatId, date, cancellationToken))
            .ToDictionary(m => m.UserId);

        await unitOfWork.ExecuteInTransactionAsync(async ct =>
        {
            foreach (var entry in entries)
            {
                if (existingByUser.TryGetValue(entry.UserId, out var meal))
                {
                    meal.BreakfastCount = entry.BreakfastCount;
                    meal.LunchCount = entry.LunchCount;
                    meal.DinnerCount = entry.DinnerCount;
                    meal.UpdatedAt = DateTime.UtcNow;
                    continue;
                }

                if (entry.BreakfastCount + entry.LunchCount + entry.DinnerCount == 0)
                    continue;

                await mealRepository.AddAsync(new Meal
                {
                    FlatId = flatId,
                    UserId = entry.UserId,
                    MealDate = date,
                    BreakfastCount = entry.BreakfastCount,
                    LunchCount = entry.LunchCount,
                    DinnerCount = entry.DinnerCount
                }, ct);
            }
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<MealSummaryDto>> GetMealTotalsByMonthAsync(
        Guid flatId,
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        var members = await memberRepository.GetByFlatIdAsync(flatId, cancellationToken);
        var meals = await mealRepository.GetByFlatAndMonthAsync(flatId, year, month, cancellationToken);
        var mealsByUser = meals.GroupBy(m => m.UserId).ToDictionary(g => g.Key, g => g.ToList());

        return members
            .Where(m => m.IsActive)
            .Select(m =>
            {
                mealsByUser.TryGetValue(m.UserId, out var userMeals);
                userMeals ??= [];
                return new MealSummaryDto
                {
                    UserId = m.UserId,
                    MemberName = m.User?.FullName ?? string.Empty,
                    TotalBreakfast = userMeals.Sum(x => x.BreakfastCount),
                    TotalLunch = userMeals.Sum(x => x.LunchCount),
                    TotalDinner = userMeals.Sum(x => x.DinnerCount),
                    GrandTotal = userMeals.Sum(x => x.TotalMealCount)
                };
            })
            .OrderBy(s => s.MemberName)
            .ToList();
    }

    private async Task ValidateEntryAsync(MealEntryDto entry, CancellationToken cancellationToken)
    {
        var result = await entryValidator.ValidateAsync(entry, cancellationToken);
        if (!result.IsValid)
            throw new BusinessException(string.Join(" ", result.Errors.Select(e => e.ErrorMessage)));
    }
}

using Messly.Application.DTOs;
using Messly.Application.Interfaces.Persistence;
using Messly.Application.Interfaces.Services;
using Messly.Domain.Entities;

namespace Messly.Application.Services;

public class BillingCalculationService(
    IExpenseRepository expenseRepository,
    IDepositRepository depositRepository,
    IMealRepository mealRepository,
    IFlatMemberRepository memberRepository,
    IRepository<MonthlySummary> monthlySummaryRepository,
    IUnitOfWork unitOfWork) : IBillingCalculationService
{
    public async Task<decimal> CalculateMealRateAsync(Guid flatId, int year, int month, CancellationToken cancellationToken = default)
    {
        var totalExpenses = await expenseRepository.GetTotalByFlatAndMonthAsync(flatId, year, month, cancellationToken);
        var meals = await mealRepository.GetByFlatAndMonthAsync(flatId, year, month, cancellationToken);
        var totalMeals = meals.Sum(m => m.TotalMealCount);

        if (totalMeals == 0) return 0;
        return Math.Round(totalExpenses / totalMeals, 2, MidpointRounding.AwayFromZero);
    }

    public async Task<IReadOnlyList<MemberBalanceDto>> GetMemberBalancesAsync(Guid flatId, int year, int month, CancellationToken cancellationToken = default)
    {
        var mealRate = await CalculateMealRateAsync(flatId, year, month, cancellationToken);
        var members = await memberRepository.GetByFlatIdAsync(flatId, cancellationToken);
        var meals = await mealRepository.GetByFlatAndMonthAsync(flatId, year, month, cancellationToken);
        var deposits = await depositRepository.GetByFlatIdAsync(flatId, cancellationToken);

        var start = new DateOnly(year, month, 1);
        var end = start.AddMonths(1).AddDays(-1);

        return members
            .Where(m => m.IsActive)
            .Select(m =>
            {
                var memberMeals = meals.Where(x => x.UserId == m.UserId).Sum(x => x.TotalMealCount);
                var mealCost = Math.Round(memberMeals * mealRate, 2, MidpointRounding.AwayFromZero);
                var memberDeposits = deposits
                    .Where(d => d.UserId == m.UserId && d.DepositDate >= start && d.DepositDate <= end)
                    .Sum(d => d.Amount);

                return new MemberBalanceDto
                {
                    UserId = m.UserId,
                    MemberName = m.User?.FullName ?? string.Empty,
                    TotalMeals = memberMeals,
                    MealCost = mealCost,
                    TotalDeposits = memberDeposits,
                    Balance = Math.Round(memberDeposits - mealCost, 2, MidpointRounding.AwayFromZero)
                };
            })
            .OrderBy(b => b.MemberName)
            .ToList();
    }

    public async Task<MonthlySummaryDto> BuildFinancialSummaryAsync(Guid flatId, int year, int month, CancellationToken cancellationToken = default)
    {
        var members = await memberRepository.GetByFlatIdAsync(flatId, cancellationToken);
        var meals = await mealRepository.GetByFlatAndMonthAsync(flatId, year, month, cancellationToken);
        var totalExpenses = await expenseRepository.GetTotalByFlatAndMonthAsync(flatId, year, month, cancellationToken);
        var totalDeposits = await depositRepository.GetTotalByFlatAndMonthAsync(flatId, year, month, cancellationToken);
        var mealRate = await CalculateMealRateAsync(flatId, year, month, cancellationToken);
        var totalMeals = meals.Sum(m => m.TotalMealCount);

        var stored = (await monthlySummaryRepository.FindAsync(
            s => s.FlatId == flatId && s.Year == year && s.Month == month, cancellationToken)).FirstOrDefault();

        return new MonthlySummaryDto
        {
            Year = year,
            Month = month,
            TotalMembers = members.Count(m => m.IsActive),
            TotalMeals = totalMeals,
            TotalExpenses = totalExpenses,
            TotalDeposits = totalDeposits,
            MealRate = mealRate,
            NetBalance = Math.Round(totalDeposits - totalExpenses, 2, MidpointRounding.AwayFromZero),
            IsFinalized = stored?.IsFinalized ?? false
        };
    }

    public async Task GenerateMonthlySummaryAsync(Guid flatId, int year, int month, CancellationToken cancellationToken = default)
    {
        var summary = await BuildFinancialSummaryAsync(flatId, year, month, cancellationToken);
        var existing = (await monthlySummaryRepository.FindAsync(
            s => s.FlatId == flatId && s.Year == year && s.Month == month, cancellationToken)).FirstOrDefault();

        if (existing is null)
        {
            await monthlySummaryRepository.AddAsync(new MonthlySummary
            {
                FlatId = flatId,
                Year = year,
                Month = month,
                TotalMembers = summary.TotalMembers,
                TotalMeals = summary.TotalMeals,
                TotalExpenses = summary.TotalExpenses,
                TotalDeposits = summary.TotalDeposits,
                MealRate = summary.MealRate,
                NetBalance = summary.NetBalance,
                IsFinalized = false
            }, cancellationToken);
        }
        else
        {
            var tracked = await monthlySummaryRepository.GetByIdForUpdateAsync(existing.Id, cancellationToken);
            if (tracked is null) return;

            tracked.TotalMembers = summary.TotalMembers;
            tracked.TotalMeals = summary.TotalMeals;
            tracked.TotalExpenses = summary.TotalExpenses;
            tracked.TotalDeposits = summary.TotalDeposits;
            tracked.MealRate = summary.MealRate;
            tracked.NetBalance = summary.NetBalance;
            tracked.UpdatedAt = DateTime.UtcNow;
            monthlySummaryRepository.Update(tracked);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

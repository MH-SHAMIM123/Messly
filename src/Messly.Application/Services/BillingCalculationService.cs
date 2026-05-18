using Messly.Application.DTOs;
using Messly.Application.Interfaces.Persistence;
using Messly.Application.Interfaces.Security;
using Messly.Application.Interfaces.Services;
using Messly.Domain.Entities;

namespace Messly.Application.Services;

public class BillingCalculationService(
    IExpenseRepository expenseRepository,
    IDepositRepository depositRepository,
    IMealRepository mealRepository,
    IFlatMemberRepository memberRepository,
    IRepository<MonthlySummary> monthlySummaryRepository,
    IUnitOfWork unitOfWork,
    IFlatAuthorizationService authorization) : IBillingCalculationService
{
    public async Task<decimal> CalculateMealRateAsync(
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        authorization.EnsureCanRead();
        var flatId = authorization.GetCurrentFlatId();
        var totalExpenses = await expenseRepository.GetTotalByFlatAndMonthAsync(flatId, year, month, cancellationToken);
        var meals = await mealRepository.GetByFlatAndMonthAsync(flatId, year, month, cancellationToken);
        var totalMeals = meals.Sum(m => m.TotalMealCount);

        if (totalMeals <= 0)
            return 0;

        return Math.Round(totalExpenses / totalMeals, 2, MidpointRounding.AwayFromZero);
    }

    public async Task<IReadOnlyList<MemberBalanceDto>> GetMemberBalancesAsync(
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        authorization.EnsureCanRead();
        var flatId = authorization.GetCurrentFlatId();
        var mealRate = await CalculateMealRateAsync(year, month, cancellationToken);
        var members = await memberRepository.GetByFlatIdAsync(flatId, cancellationToken);
        var meals = await mealRepository.GetByFlatAndMonthAsync(flatId, year, month, cancellationToken);
        var deposits = await depositRepository.GetByFlatAndMonthAsync(flatId, year, month, cancellationToken);

        return BuildMemberBalances(members, meals, deposits, mealRate);
    }

    public async Task<MonthlySummaryDto> BuildFinancialSummaryAsync(
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        authorization.EnsureCanRead();
        var flatId = authorization.GetCurrentFlatId();
        var members = await memberRepository.GetByFlatIdAsync(flatId, cancellationToken);
        var meals = await mealRepository.GetByFlatAndMonthAsync(flatId, year, month, cancellationToken);
        var deposits = await depositRepository.GetByFlatAndMonthAsync(flatId, year, month, cancellationToken);
        var totalExpenses = await expenseRepository.GetTotalByFlatAndMonthAsync(flatId, year, month, cancellationToken);
        var totalDeposits = deposits.Sum(d => d.Amount);
        var totalMeals = meals.Sum(m => m.TotalMealCount);
        var hasMeals = totalMeals > 0;
        var hasExpenses = totalExpenses > 0;
        var mealRate = hasMeals
            ? Math.Round(totalExpenses / totalMeals, 2, MidpointRounding.AwayFromZero)
            : 0;

        var stored = (await monthlySummaryRepository.FindAsync(
            s => s.FlatId == flatId && s.Year == year && s.Month == month,
            cancellationToken)).FirstOrDefault();

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
            IsFinalized = stored?.IsFinalized ?? false,
            HasMeals = hasMeals,
            HasExpenses = hasExpenses,
            CalculationNote = BuildCalculationNote(hasMeals, hasExpenses),
            MemberBalances = BuildMemberBalances(members, meals, deposits, mealRate)
        };
    }

    public async Task GenerateMonthlySummaryAsync(
        int year,
        int month,
        CancellationToken cancellationToken = default)
    {
        authorization.EnsureManager();
        var flatId = authorization.GetCurrentFlatId();
        var summary = await BuildFinancialSummaryAsync(year, month, cancellationToken);
        var existing = (await monthlySummaryRepository.FindAsync(
            s => s.FlatId == flatId && s.Year == year && s.Month == month,
            cancellationToken)).FirstOrDefault();

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
            if (tracked is null || tracked.FlatId != flatId)
                return;

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

    private static IReadOnlyList<MemberBalanceDto> BuildMemberBalances(
        IReadOnlyList<FlatMember> members,
        IReadOnlyList<Meal> meals,
        IReadOnlyList<Deposit> deposits,
        decimal mealRate)
    {
        var mealsByUser = meals.GroupBy(m => m.UserId).ToDictionary(g => g.Key, g => g.Sum(x => x.TotalMealCount));
        var depositsByUser = deposits.GroupBy(d => d.UserId).ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        return members
            .Where(m => m.IsActive)
            .Select(m =>
            {
                mealsByUser.TryGetValue(m.UserId, out var totalMeals);
                depositsByUser.TryGetValue(m.UserId, out var totalDeposits);
                var mealCost = Math.Round(totalMeals * mealRate, 2, MidpointRounding.AwayFromZero);
                var balance = Math.Round(totalDeposits - mealCost, 2, MidpointRounding.AwayFromZero);

                return new MemberBalanceDto
                {
                    UserId = m.UserId,
                    MemberName = m.User?.FullName ?? string.Empty,
                    TotalMeals = totalMeals,
                    MealCost = mealCost,
                    TotalDeposits = totalDeposits,
                    Balance = balance
                };
            })
            .OrderBy(b => b.MemberName)
            .ToList();
    }

    private static string? BuildCalculationNote(bool hasMeals, bool hasExpenses)
    {
        if (!hasMeals && !hasExpenses)
            return "No expenses or meals recorded for this month. Meal rate is 0.";

        if (!hasMeals)
            return "No meals recorded for this month. Meal rate is 0 (expenses cannot be allocated).";

        if (!hasExpenses)
            return "No expenses recorded for this month. Meal rate is 0.";

        return null;
    }
}

using Messly.Application.DTOs;
using Messly.Application.Interfaces.Persistence;
using Messly.Application.Interfaces.Security;
using Messly.Application.Interfaces.Services;

namespace Messly.Application.Services;

public class DashboardService(
    IFlatMemberRepository memberRepository,
    IExpenseRepository expenseRepository,
    IMealRepository mealRepository,
    IDepositRepository depositRepository,
    IBillingCalculationService billingCalculationService,
    IFlatService flatService,
    IFlatAuthorizationService authorization) : IDashboardService
{
    public async Task<DashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default)
    {
        authorization.EnsureCanRead();
        var flatId = authorization.GetCurrentFlatId();
        var now = DateTime.UtcNow;
        var members = await memberRepository.GetByFlatIdAsync(flatId, cancellationToken);
        var totalExpense = await expenseRepository.GetTotalByFlatAndMonthAsync(flatId, now.Year, now.Month, cancellationToken);
        var meals = await mealRepository.GetByFlatAndMonthAsync(flatId, now.Year, now.Month, cancellationToken);
        var mealRate = await billingCalculationService.CalculateMealRateAsync(now.Year, now.Month, cancellationToken);
        var flat = await flatService.GetFlatSettingsAsync(cancellationToken);

        return new DashboardDto
        {
            TotalMembers = members.Count(m => m.IsActive),
            TotalExpense = totalExpense,
            TotalMeals = meals.Sum(m => m.TotalMealCount),
            MealRate = mealRate > 0 ? mealRate : flat?.DefaultMealRate ?? 0,
            TotalDeposits = await depositRepository.GetTotalByFlatAndMonthAsync(flatId, now.Year, now.Month, cancellationToken)
        };
    }

    public async Task<MonthlySummaryDto?> GetFinancialSummaryAsync(int year, int month, CancellationToken cancellationToken = default)
        => await billingCalculationService.BuildFinancialSummaryAsync(year, month, cancellationToken);

    public Task<IReadOnlyList<MemberBalanceDto>> GetMemberBalancesAsync(int year, int month, CancellationToken cancellationToken = default)
        => billingCalculationService.GetMemberBalancesAsync(year, month, cancellationToken);

    public Task<FlatDto?> GetFlatSettingsAsync(CancellationToken cancellationToken = default)
        => flatService.GetFlatSettingsAsync(cancellationToken);

    public Task SaveFlatSettingsAsync(FlatDto dto, CancellationToken cancellationToken = default)
        => flatService.SaveFlatSettingsAsync(dto, cancellationToken);
}

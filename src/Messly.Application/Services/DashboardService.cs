using Messly.Application.DTOs;
using Messly.Application.Interfaces.Persistence;
using Messly.Application.Interfaces.Services;

namespace Messly.Application.Services;

public class DashboardService(
    IFlatMemberRepository memberRepository,
    IExpenseRepository expenseRepository,
    IMealRepository mealRepository,
    IDepositRepository depositRepository,
    IBillingCalculationService billingCalculationService,
    IFlatService flatService) : IDashboardService
{
    public async Task<DashboardDto> GetDashboardAsync(Guid flatId, CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;
        var members = await memberRepository.GetByFlatIdAsync(flatId, cancellationToken);
        var totalExpense = await expenseRepository.GetTotalByFlatAndMonthAsync(flatId, now.Year, now.Month, cancellationToken);
        var meals = await mealRepository.GetByFlatAndMonthAsync(flatId, now.Year, now.Month, cancellationToken);
        var mealRate = await billingCalculationService.CalculateMealRateAsync(flatId, now.Year, now.Month, cancellationToken);
        var flat = await flatService.GetFlatSettingsAsync(flatId, cancellationToken);

        return new DashboardDto
        {
            TotalMembers = members.Count(m => m.IsActive),
            TotalExpense = totalExpense,
            TotalMeals = meals.Sum(m => m.TotalMealCount),
            MealRate = mealRate > 0 ? mealRate : flat?.DefaultMealRate ?? 0,
            TotalDeposits = await depositRepository.GetTotalByFlatAndMonthAsync(flatId, now.Year, now.Month, cancellationToken)
        };
    }

    public async Task<MonthlySummaryDto?> GetFinancialSummaryAsync(Guid flatId, int year, int month, CancellationToken cancellationToken = default)
        => await billingCalculationService.BuildFinancialSummaryAsync(flatId, year, month, cancellationToken);

    public Task<IReadOnlyList<MemberBalanceDto>> GetMemberBalancesAsync(Guid flatId, int year, int month, CancellationToken cancellationToken = default)
        => billingCalculationService.GetMemberBalancesAsync(flatId, year, month, cancellationToken);

    public Task<FlatDto?> GetFlatSettingsAsync(Guid flatId, CancellationToken cancellationToken = default)
        => flatService.GetFlatSettingsAsync(flatId, cancellationToken);

    public Task SaveFlatSettingsAsync(FlatDto dto, CancellationToken cancellationToken = default)
        => flatService.SaveFlatSettingsAsync(dto, cancellationToken);
}

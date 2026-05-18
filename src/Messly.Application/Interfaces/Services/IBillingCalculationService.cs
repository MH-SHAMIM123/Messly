using Messly.Application.DTOs;

namespace Messly.Application.Interfaces.Services;

public interface IBillingCalculationService
{
    Task<decimal> CalculateMealRateAsync(int year, int month, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MemberBalanceDto>> GetMemberBalancesAsync(int year, int month, CancellationToken cancellationToken = default);
    Task<MonthlySummaryDto> BuildFinancialSummaryAsync(int year, int month, CancellationToken cancellationToken = default);
    Task GenerateMonthlySummaryAsync(int year, int month, CancellationToken cancellationToken = default);
}

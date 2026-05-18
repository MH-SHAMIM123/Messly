using Messly.Application.DTOs;

namespace Messly.Application.Interfaces.Services;

public interface IBillingCalculationService
{
    Task<decimal> CalculateMealRateAsync(Guid flatId, int year, int month, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MemberBalanceDto>> GetMemberBalancesAsync(Guid flatId, int year, int month, CancellationToken cancellationToken = default);
    Task<MonthlySummaryDto> BuildFinancialSummaryAsync(Guid flatId, int year, int month, CancellationToken cancellationToken = default);
    Task GenerateMonthlySummaryAsync(Guid flatId, int year, int month, CancellationToken cancellationToken = default);
}

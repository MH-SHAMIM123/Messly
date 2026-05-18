using Messly.Application.DTOs;

namespace Messly.Application.Interfaces.Services;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync(CancellationToken cancellationToken = default);
    Task<MonthlySummaryDto?> GetFinancialSummaryAsync(int year, int month, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MemberBalanceDto>> GetMemberBalancesAsync(int year, int month, CancellationToken cancellationToken = default);
    Task<FlatDto?> GetFlatSettingsAsync(CancellationToken cancellationToken = default);
    Task SaveFlatSettingsAsync(FlatDto dto, CancellationToken cancellationToken = default);
}

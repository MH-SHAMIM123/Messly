using Messly.Application.DTOs;

namespace Messly.Application.Interfaces.Services;

public interface IDashboardService
{
    Task<DashboardDto> GetDashboardAsync(Guid flatId, CancellationToken cancellationToken = default);
    Task<MonthlySummaryDto?> GetFinancialSummaryAsync(Guid flatId, int year, int month, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<MemberBalanceDto>> GetMemberBalancesAsync(Guid flatId, int year, int month, CancellationToken cancellationToken = default);
    Task<FlatDto?> GetFlatSettingsAsync(Guid flatId, CancellationToken cancellationToken = default);
    Task SaveFlatSettingsAsync(FlatDto dto, CancellationToken cancellationToken = default);
}

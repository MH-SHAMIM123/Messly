using Messly.Application.DTOs;

namespace Messly.Application.Interfaces.Services;

public interface IDepositService
{
    Task<IReadOnlyList<DepositDto>> GetDepositsAsync(Guid flatId, CancellationToken cancellationToken = default);
    Task<DepositDto?> GetDepositAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guid> SaveDepositAsync(DepositUpsertDto dto, CancellationToken cancellationToken = default);
    Task DeleteDepositAsync(Guid id, CancellationToken cancellationToken = default);
}

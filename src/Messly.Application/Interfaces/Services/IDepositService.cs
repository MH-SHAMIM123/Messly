using Messly.Application.DTOs;

namespace Messly.Application.Interfaces.Services;

public interface IDepositService
{
    Task<IReadOnlyList<DepositDto>> GetDepositsAsync(CancellationToken cancellationToken = default);
    Task<DepositDto?> GetDepositAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guid> CreateDepositAsync(DepositUpsertDto dto, CancellationToken cancellationToken = default);
    Task UpdateDepositAsync(DepositUpsertDto dto, CancellationToken cancellationToken = default);
    Task<Guid> SaveDepositAsync(DepositUpsertDto dto, CancellationToken cancellationToken = default);
    Task DeleteDepositAsync(Guid id, CancellationToken cancellationToken = default);
}

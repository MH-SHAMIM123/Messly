using Messly.Application.Common;
using Messly.Application.DTOs;
using Messly.Application.Interfaces.Persistence;
using Messly.Application.Interfaces.Services;
using Messly.Domain.Entities;

namespace Messly.Application.Services;

public class DepositService(IDepositRepository depositRepository, IUnitOfWork unitOfWork) : IDepositService
{
    public async Task<IReadOnlyList<DepositDto>> GetDepositsAsync(Guid flatId, CancellationToken cancellationToken = default)
    {
        var deposits = await depositRepository.GetByFlatIdAsync(flatId, cancellationToken);
        return deposits.Select(MapToDto).ToList();
    }

    public async Task<DepositDto?> GetDepositAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deposit = await depositRepository.GetByIdAsync(id, cancellationToken);
        return deposit is null ? null : MapToDto(deposit);
    }

    public async Task<Guid> SaveDepositAsync(DepositUpsertDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Amount <= 0)
            throw new BusinessException("Amount must be greater than zero.");
        if (dto.UserId == Guid.Empty)
            throw new BusinessException("Member is required.");

        if (dto.Id.HasValue)
        {
            var deposit = await depositRepository.GetByIdForUpdateAsync(dto.Id.Value, cancellationToken)
                ?? throw new BusinessException("Deposit not found.");

            deposit.UserId = dto.UserId;
            deposit.Amount = dto.Amount;
            deposit.DepositDate = dto.DepositDate;
            deposit.Notes = dto.Notes?.Trim();
            deposit.ReferenceNumber = dto.ReferenceNumber?.Trim();
            deposit.UpdatedAt = DateTime.UtcNow;
            depositRepository.Update(deposit);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return deposit.Id;
        }

        var entity = new Deposit
        {
            FlatId = dto.FlatId,
            UserId = dto.UserId,
            Amount = dto.Amount,
            DepositDate = dto.DepositDate,
            Notes = dto.Notes?.Trim(),
            ReferenceNumber = dto.ReferenceNumber?.Trim()
        };
        await depositRepository.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    public async Task DeleteDepositAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deposit = await depositRepository.GetByIdForUpdateAsync(id, cancellationToken);
        if (deposit is null) return;

        depositRepository.Remove(deposit);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private static DepositDto MapToDto(Deposit deposit) => new()
    {
        Id = deposit.Id,
        FlatId = deposit.FlatId,
        UserId = deposit.UserId,
        MemberName = deposit.User?.FullName ?? string.Empty,
        Amount = deposit.Amount,
        DepositDate = deposit.DepositDate,
        Notes = deposit.Notes,
        ReferenceNumber = deposit.ReferenceNumber
    };
}

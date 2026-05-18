using FluentValidation;
using Messly.Application.Common;
using Messly.Application.DTOs;
using Messly.Application.Interfaces.Persistence;
using Messly.Application.Interfaces.Services;
using Messly.Domain.Entities;

namespace Messly.Application.Services;

public class DepositService(
    IDepositRepository depositRepository,
    IUnitOfWork unitOfWork,
    IValidator<DepositUpsertDto> validator) : IDepositService
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
        if (dto.Id.HasValue)
        {
            await UpdateDepositAsync(dto, cancellationToken);
            return dto.Id.Value;
        }

        return await CreateDepositAsync(dto, cancellationToken);
    }

    public async Task<Guid> CreateDepositAsync(DepositUpsertDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Id.HasValue)
            throw new BusinessException("Cannot create a deposit with an existing id. Use update instead.");

        await ValidateAsync(dto, cancellationToken);

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

    public async Task UpdateDepositAsync(DepositUpsertDto dto, CancellationToken cancellationToken = default)
    {
        if (!dto.Id.HasValue)
            throw new BusinessException("Deposit id is required for update.");

        await ValidateAsync(dto, cancellationToken);

        var deposit = await depositRepository.GetByIdForUpdateAsync(dto.Id.Value, cancellationToken)
            ?? throw new BusinessException("Deposit not found.");

        if (deposit.FlatId != dto.FlatId)
            throw new BusinessException("Deposit does not belong to this flat.");

        deposit.UserId = dto.UserId;
        deposit.Amount = dto.Amount;
        deposit.DepositDate = dto.DepositDate;
        deposit.Notes = dto.Notes?.Trim();
        deposit.ReferenceNumber = dto.ReferenceNumber?.Trim();
        deposit.UpdatedAt = DateTime.UtcNow;
        depositRepository.Update(deposit);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteDepositAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var deposit = await depositRepository.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new BusinessException("Deposit not found.");

        depositRepository.Remove(deposit);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task ValidateAsync(DepositUpsertDto dto, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            throw new BusinessException(string.Join(" ", validation.Errors.Select(e => e.ErrorMessage)));
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

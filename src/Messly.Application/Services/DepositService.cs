using FluentValidation;
using Messly.Application.Common;
using Messly.Application.DTOs;
using Messly.Application.Interfaces.Persistence;
using Messly.Application.Interfaces.Security;
using Messly.Application.Interfaces.Services;
using Messly.Domain.Entities;

namespace Messly.Application.Services;

public class DepositService(
    IDepositRepository depositRepository,
    IUnitOfWork unitOfWork,
    IValidator<DepositUpsertDto> validator,
    IFlatAuthorizationService authorization) : IDepositService
{
    public async Task<IReadOnlyList<DepositDto>> GetDepositsAsync(CancellationToken cancellationToken = default)
    {
        authorization.EnsureCanRead();
        var flatId = authorization.GetCurrentFlatId();
        var deposits = await depositRepository.GetByFlatIdAsync(flatId, cancellationToken);
        return deposits.Select(MapToDto).ToList();
    }

    public async Task<DepositDto?> GetDepositAsync(Guid id, CancellationToken cancellationToken = default)
    {
        authorization.EnsureCanRead();
        var flatId = authorization.GetCurrentFlatId();
        var deposit = await depositRepository.GetByIdAndFlatAsync(id, flatId, cancellationToken);
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
        authorization.EnsureManager();
        var flatId = authorization.GetCurrentFlatId();

        if (dto.Id.HasValue)
            throw new BusinessException("Cannot create a deposit with an existing id. Use update instead.");

        await ValidateAsync(dto, cancellationToken);
        await authorization.EnsureUserIsActiveMemberAsync(dto.UserId, cancellationToken);

        var entity = new Deposit
        {
            FlatId = flatId,
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
        authorization.EnsureManager();
        var flatId = authorization.GetCurrentFlatId();

        if (!dto.Id.HasValue)
            throw new BusinessException("Deposit id is required for update.");

        await ValidateAsync(dto, cancellationToken);
        await authorization.EnsureUserIsActiveMemberAsync(dto.UserId, cancellationToken);

        var deposit = await depositRepository.GetByIdForUpdateAndFlatAsync(dto.Id.Value, flatId, cancellationToken)
            ?? throw new NotFoundException();

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
        authorization.EnsureManager();
        var flatId = authorization.GetCurrentFlatId();

        var deposit = await depositRepository.GetByIdForUpdateAndFlatAsync(id, flatId, cancellationToken)
            ?? throw new NotFoundException();

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

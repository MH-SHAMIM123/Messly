using Messly.Application.Common;
using Messly.Application.DTOs;
using Messly.Application.Interfaces.Persistence;
using Messly.Application.Interfaces.Services;
using Messly.Domain.Entities;

namespace Messly.Application.Services;

public class FlatService(IRepository<Flat> flatRepository, IUnitOfWork unitOfWork) : IFlatService
{
    public async Task<FlatDto?> GetFlatSettingsAsync(Guid flatId, CancellationToken cancellationToken = default)
    {
        var flat = await flatRepository.GetByIdAsync(flatId, cancellationToken);
        if (flat is null) return null;

        return new FlatDto
        {
            Id = flat.Id,
            Name = flat.Name,
            Address = flat.Address,
            Description = flat.Description,
            DefaultMealRate = flat.DefaultMealRate,
            BillingDayOfMonth = flat.BillingDayOfMonth
        };
    }

    public async Task SaveFlatSettingsAsync(FlatDto dto, CancellationToken cancellationToken = default)
    {
        var flat = await flatRepository.GetByIdForUpdateAsync(dto.Id, cancellationToken)
            ?? throw new BusinessException("Flat not found.");

        flat.Name = dto.Name.Trim();
        flat.Address = dto.Address?.Trim();
        flat.Description = dto.Description?.Trim();
        flat.DefaultMealRate = dto.DefaultMealRate;
        flat.BillingDayOfMonth = dto.BillingDayOfMonth;
        flat.UpdatedAt = DateTime.UtcNow;

        flatRepository.Update(flat);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }
}

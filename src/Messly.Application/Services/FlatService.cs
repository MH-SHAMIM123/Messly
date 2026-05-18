using Messly.Application.Common;
using Messly.Application.DTOs;
using Messly.Application.Interfaces.Persistence;
using Messly.Application.Interfaces.Security;
using Messly.Application.Interfaces.Services;

namespace Messly.Application.Services;

public class FlatService(
    IRepository<Domain.Entities.Flat> flatRepository,
    IUnitOfWork unitOfWork,
    IFlatAuthorizationService authorization) : IFlatService
{
    public async Task<FlatDto?> GetFlatSettingsAsync(CancellationToken cancellationToken = default)
    {
        authorization.EnsureCanRead();
        var flatId = authorization.GetCurrentFlatId();
        var flat = await flatRepository.GetByIdAsync(flatId, cancellationToken);
        if (flat is null || flat.Id != flatId)
            return null;

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
        authorization.EnsureManager();
        var flatId = authorization.GetCurrentFlatId();

        if (dto.Id != flatId)
            throw new ForbiddenException();

        var flat = await flatRepository.GetByIdForUpdateAsync(flatId, cancellationToken)
            ?? throw new NotFoundException();

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

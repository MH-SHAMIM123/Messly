using Messly.Application.DTOs;

namespace Messly.Application.Interfaces.Services;

public interface IFlatService
{
    Task<FlatDto?> GetFlatSettingsAsync(Guid flatId, CancellationToken cancellationToken = default);
    Task SaveFlatSettingsAsync(FlatDto dto, CancellationToken cancellationToken = default);
}

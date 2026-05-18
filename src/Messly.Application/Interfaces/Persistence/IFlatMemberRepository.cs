using Messly.Domain.Entities;

namespace Messly.Application.Interfaces.Persistence;

public interface IFlatMemberRepository : IRepository<FlatMember>
{
    Task<IReadOnlyList<FlatMember>> GetByFlatIdAsync(Guid flatId, CancellationToken cancellationToken = default);
    Task<FlatMember?> GetByIdWithDetailsAsync(Guid id, CancellationToken cancellationToken = default);
    Task<int> CountActiveManagersAsync(Guid flatId, CancellationToken cancellationToken = default);
}

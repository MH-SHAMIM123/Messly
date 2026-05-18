using Messly.Application.DTOs;

namespace Messly.Application.Interfaces.Services;

public interface IMemberService
{
    Task<IReadOnlyList<MemberDto>> GetMembersAsync(Guid flatId, CancellationToken cancellationToken = default);
    Task<MemberDto?> GetMemberAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guid> SaveMemberAsync(MemberUpsertDto dto, CancellationToken cancellationToken = default);
    Task DeleteMemberAsync(Guid id, CancellationToken cancellationToken = default);
}

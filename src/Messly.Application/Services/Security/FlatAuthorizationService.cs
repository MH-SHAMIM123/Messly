using Messly.Application.Common;
using Messly.Application.Interfaces.Persistence;
using Messly.Application.Interfaces.Security;
using Messly.Domain.Enums;

namespace Messly.Application.Services.Security;

public class FlatAuthorizationService(
    ITenantContext tenantContext,
    IFlatMemberRepository memberRepository) : IFlatAuthorizationService
{
    public Guid GetCurrentFlatId()
    {
        if (!tenantContext.IsAuthenticated || tenantContext.FlatId == Guid.Empty)
            throw new ForbiddenException();

        return tenantContext.FlatId;
    }

    public void EnsureCanRead()
    {
        if (!tenantContext.IsAuthenticated || tenantContext.FlatId == Guid.Empty)
            throw new ForbiddenException();
    }

    public void EnsureCanWrite()
    {
        EnsureCanRead();
        if (!tenantContext.IsManager)
            throw new ForbiddenException();
    }

    public void EnsureManager() => EnsureCanWrite();

    public async Task EnsureUserIsActiveMemberAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var flatId = GetCurrentFlatId();
        if (!await memberRepository.IsActiveMemberOfFlatAsync(flatId, userId, cancellationToken))
            throw new BusinessException("The selected member is not part of this flat.");
    }
}

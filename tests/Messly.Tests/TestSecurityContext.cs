using Messly.Application.Interfaces.Security;
using Messly.Domain.Enums;

namespace Messly.Tests;

internal sealed class TestTenantContext(Guid flatId, RoleType roleType, Guid domainUserId) : ITenantContext
{
    public Guid FlatId { get; } = flatId;
    public Guid DomainUserId { get; } = domainUserId;
    public RoleType RoleType { get; } = roleType;
    public bool IsManager => RoleType == RoleType.Manager;
    public bool IsAuthenticated => true;
}

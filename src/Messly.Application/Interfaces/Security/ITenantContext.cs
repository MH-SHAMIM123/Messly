using Messly.Domain.Enums;

namespace Messly.Application.Interfaces.Security;

public interface ITenantContext
{
    Guid FlatId { get; }
    Guid DomainUserId { get; }
    RoleType RoleType { get; }
    bool IsManager { get; }
    bool IsAuthenticated { get; }
}

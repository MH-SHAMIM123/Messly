using System.Security.Claims;
using Messly.Application.Interfaces.Security;
using Messly.Domain.Enums;

namespace Messly.Api.Services;

public class TenantContext(IHttpContextAccessor httpContextAccessor) : ITenantContext
{
    public const string FlatIdClaimType = "flat_id";
    public const string DomainUserIdClaimType = "domain_user_id";

    public bool IsAuthenticated =>
        httpContextAccessor.HttpContext?.User?.Identity?.IsAuthenticated == true;

    public Guid FlatId
    {
        get
        {
            var claim = httpContextAccessor.HttpContext?.User?.FindFirst(FlatIdClaimType)?.Value;
            return Guid.TryParse(claim, out var flatId) ? flatId : Guid.Empty;
        }
    }

    public Guid DomainUserId
    {
        get
        {
            var claim = httpContextAccessor.HttpContext?.User?.FindFirst(DomainUserIdClaimType)?.Value;
            return Guid.TryParse(claim, out var userId) ? userId : Guid.Empty;
        }
    }

    public RoleType RoleType
    {
        get
        {
            var role = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.Role)?.Value;
            return Enum.TryParse<RoleType>(role, out var roleType) ? roleType : RoleType.Member;
        }
    }

    public bool IsManager => RoleType == RoleType.Manager;
}

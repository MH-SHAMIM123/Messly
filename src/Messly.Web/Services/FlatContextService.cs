using System.Security.Claims;
using Messly.Infrastructure.Data;

namespace Messly.Web.Services;

public class FlatContextService(IConfiguration configuration, IHttpContextAccessor httpContextAccessor)
{
    public Guid CurrentFlatId
    {
        get
        {
            var claim = httpContextAccessor.HttpContext?.User?.FindFirst("flat_id")?.Value;
            if (Guid.TryParse(claim, out var flatId))
                return flatId;

            return Guid.TryParse(configuration["Messly:DefaultFlatId"], out var configured)
                ? configured
                : DevDataSeeder.DefaultFlatId;
        }
    }

    public Guid? CurrentUserId
    {
        get
        {
            var claim = httpContextAccessor.HttpContext?.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var id) ? id : null;
        }
    }

    public void SetFlat(Guid flatId) { /* set via claims after login */ }
}

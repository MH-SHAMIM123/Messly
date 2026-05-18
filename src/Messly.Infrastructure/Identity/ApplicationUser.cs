using Microsoft.AspNetCore.Identity;

namespace Messly.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>
{
    public Guid? DomainUserId { get; set; }
}

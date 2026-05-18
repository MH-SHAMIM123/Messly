using Messly.Domain.Common;

namespace Messly.Domain.Entities;

public class FlatMember : BaseEntity
{
    public Guid FlatId { get; set; }
    public Flat Flat { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public Guid RoleId { get; set; }
    public Role Role { get; set; } = null!;

    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
}

using Messly.Domain.Common;
using Messly.Domain.Enums;

namespace Messly.Domain.Entities;

public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public RoleType RoleType { get; set; }

    public ICollection<FlatMember> FlatMembers { get; set; } = new List<FlatMember>();
}

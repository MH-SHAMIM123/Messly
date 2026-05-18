using Messly.Domain.Enums;

namespace Messly.Application.DTOs;

public class MemberDto
{
    public Guid Id { get; set; }
    public Guid FlatId { get; set; }
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public RoleType RoleType { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public bool IsActive { get; set; }
}

public class MemberUpsertDto
{
    public Guid? Id { get; set; }
    public Guid FlatId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public RoleType RoleType { get; set; } = RoleType.Member;
    public bool IsActive { get; set; } = true;
}

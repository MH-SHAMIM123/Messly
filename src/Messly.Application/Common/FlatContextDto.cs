namespace Messly.Application.Common;

/// <summary>
/// Represents the active flat context for manager operations (skeleton — wired later).
/// </summary>
public class FlatContextDto
{
    public Guid FlatId { get; set; }
    public string FlatName { get; set; } = string.Empty;
    public Guid ManagerUserId { get; set; }
}

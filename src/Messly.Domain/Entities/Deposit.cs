using Messly.Domain.Common;

namespace Messly.Domain.Entities;

public class Deposit : BaseEntity
{
    public Guid FlatId { get; set; }
    public Flat Flat { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public decimal Amount { get; set; }
    public DateOnly DepositDate { get; set; }
    public string? Notes { get; set; }
    public string? ReferenceNumber { get; set; }
}

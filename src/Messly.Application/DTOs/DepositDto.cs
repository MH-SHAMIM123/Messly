namespace Messly.Application.DTOs;

public class DepositDto
{
    public Guid Id { get; set; }
    public Guid FlatId { get; set; }
    public Guid UserId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateOnly DepositDate { get; set; }
    public string? Notes { get; set; }
    public string? ReferenceNumber { get; set; }
}

public class DepositUpsertDto
{
    public Guid? Id { get; set; }
    public Guid FlatId { get; set; }
    public Guid UserId { get; set; }
    public decimal Amount { get; set; }
    public DateOnly DepositDate { get; set; }
    public string? Notes { get; set; }
    public string? ReferenceNumber { get; set; }
}

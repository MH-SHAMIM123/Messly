namespace Messly.Application.DTOs;

public class FlatDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Description { get; set; }
    public decimal DefaultMealRate { get; set; }
    public int BillingDayOfMonth { get; set; }
}

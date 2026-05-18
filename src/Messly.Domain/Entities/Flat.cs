using Messly.Domain.Common;

namespace Messly.Domain.Entities;

public class Flat : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? Description { get; set; }
    public decimal DefaultMealRate { get; set; }
    public int BillingDayOfMonth { get; set; } = 1;

    public Guid CreatorId { get; set; }
    public User Creator { get; set; } = null!;

    public ICollection<FlatMember> Members { get; set; } = new List<FlatMember>();
    public ICollection<Meal> Meals { get; set; } = new List<Meal>();
    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
    public ICollection<Deposit> Deposits { get; set; } = new List<Deposit>();
    public ICollection<ExpenseCategory> ExpenseCategories { get; set; } = new List<ExpenseCategory>();
    public ICollection<MonthlySummary> MonthlySummaries { get; set; } = new List<MonthlySummary>();
}

using Messly.Domain.Common;
using Messly.Domain.Enums;

namespace Messly.Domain.Entities;

public class Expense : BaseEntity
{
    public Guid FlatId { get; set; }
    public Flat Flat { get; set; } = null!;

    public Guid PaidByUserId { get; set; }
    public User PaidBy { get; set; } = null!;

    public Guid ExpenseCategoryId { get; set; }
    public ExpenseCategory Category { get; set; } = null!;

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateOnly ExpenseDate { get; set; }
    public ExpenseType ExpenseType { get; set; }
}

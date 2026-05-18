using Messly.Domain.Common;

namespace Messly.Domain.Entities;

public class ExpenseCategory : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public Guid FlatId { get; set; }
    public Flat Flat { get; set; } = null!;

    public ICollection<Expense> Expenses { get; set; } = new List<Expense>();
}

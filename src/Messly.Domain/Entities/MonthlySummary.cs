using Messly.Domain.Common;

namespace Messly.Domain.Entities;

public class MonthlySummary : BaseEntity
{
    public Guid FlatId { get; set; }
    public Flat Flat { get; set; } = null!;

    public int Year { get; set; }
    public int Month { get; set; }
    public int TotalMembers { get; set; }
    public int TotalMeals { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal TotalDeposits { get; set; }
    public decimal MealRate { get; set; }
    public decimal NetBalance { get; set; }
    public bool IsFinalized { get; set; }
}

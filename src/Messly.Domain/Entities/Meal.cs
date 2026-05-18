using Messly.Domain.Common;

namespace Messly.Domain.Entities;

public class Meal : BaseEntity
{
    public Guid FlatId { get; set; }
    public Flat Flat { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public DateOnly MealDate { get; set; }
    public int BreakfastCount { get; set; }
    public int LunchCount { get; set; }
    public int DinnerCount { get; set; }
    public string? Notes { get; set; }

    public int TotalMealCount => BreakfastCount + LunchCount + DinnerCount;
}

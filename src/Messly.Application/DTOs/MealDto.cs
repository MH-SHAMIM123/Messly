namespace Messly.Application.DTOs;

public class MealDto
{
    public Guid Id { get; set; }
    public Guid FlatId { get; set; }
    public Guid UserId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public DateOnly MealDate { get; set; }
    public int BreakfastCount { get; set; }
    public int LunchCount { get; set; }
    public int DinnerCount { get; set; }
    public int TotalMealCount { get; set; }
}

public class MealEntryDto
{
    public Guid UserId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public int BreakfastCount { get; set; }
    public int LunchCount { get; set; }
    public int DinnerCount { get; set; }
}

public class MealSummaryDto
{
    public Guid UserId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public int TotalBreakfast { get; set; }
    public int TotalLunch { get; set; }
    public int TotalDinner { get; set; }
    public int GrandTotal { get; set; }
}

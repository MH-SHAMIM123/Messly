namespace Messly.Application.DTOs;

public class DashboardDto
{
    public int TotalMembers { get; set; }
    public decimal TotalExpense { get; set; }
    public int TotalMeals { get; set; }
    public decimal MealRate { get; set; }
    public decimal TotalDeposits { get; set; }
}

public class MonthlySummaryDto
{
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

public class MemberBalanceDto
{
    public Guid UserId { get; set; }
    public string MemberName { get; set; } = string.Empty;
    public int TotalMeals { get; set; }
    public decimal MealCost { get; set; }
    public decimal TotalDeposits { get; set; }
    public decimal Balance { get; set; }
}

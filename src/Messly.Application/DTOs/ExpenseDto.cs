using Messly.Domain.Enums;

namespace Messly.Application.DTOs;

public class ExpenseDto
{
    public Guid Id { get; set; }
    public Guid FlatId { get; set; }
    public Guid PaidByUserId { get; set; }
    public Guid ExpenseCategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateOnly ExpenseDate { get; set; }
    public ExpenseType ExpenseType { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string PaidByName { get; set; } = string.Empty;
}

public class ExpenseUpsertDto
{
    public Guid? Id { get; set; }
    public Guid FlatId { get; set; }
    public Guid PaidByUserId { get; set; }
    public Guid ExpenseCategoryId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Amount { get; set; }
    public DateOnly ExpenseDate { get; set; }
    public ExpenseType ExpenseType { get; set; }
}

public class ExpenseCategoryDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

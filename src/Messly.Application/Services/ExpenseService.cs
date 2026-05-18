using Messly.Application.Common;
using Messly.Application.DTOs;
using Messly.Application.Interfaces.Persistence;
using Messly.Application.Interfaces.Services;
using Messly.Domain.Entities;

namespace Messly.Application.Services;

public class ExpenseService(
    IExpenseRepository expenseRepository,
    IExpenseCategoryRepository categoryRepository,
    IUnitOfWork unitOfWork) : IExpenseService
{
    public async Task<IReadOnlyList<ExpenseDto>> GetExpensesAsync(Guid flatId, CancellationToken cancellationToken = default)
    {
        var expenses = await expenseRepository.GetByFlatIdAsync(flatId, cancellationToken);
        return expenses.Select(MapToDto).ToList();
    }

    public async Task<ExpenseDto?> GetExpenseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var expense = await expenseRepository.GetByIdAsync(id, cancellationToken);
        return expense is null ? null : MapToDto(expense);
    }

    public async Task<Guid> SaveExpenseAsync(ExpenseUpsertDto dto, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(dto.Title))
            throw new BusinessException("Title is required.");
        if (dto.Amount <= 0)
            throw new BusinessException("Amount must be greater than zero.");

        if (dto.Id.HasValue)
        {
            var expense = await expenseRepository.GetByIdForUpdateAsync(dto.Id.Value, cancellationToken)
                ?? throw new BusinessException("Expense not found.");

            expense.Title = dto.Title.Trim();
            expense.Description = dto.Description?.Trim();
            expense.Amount = dto.Amount;
            expense.ExpenseDate = dto.ExpenseDate;
            expense.ExpenseType = dto.ExpenseType;
            expense.PaidByUserId = dto.PaidByUserId;
            expense.ExpenseCategoryId = dto.ExpenseCategoryId;
            expense.UpdatedAt = DateTime.UtcNow;
            expenseRepository.Update(expense);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            return expense.Id;
        }

        var entity = new Expense
        {
            FlatId = dto.FlatId,
            PaidByUserId = dto.PaidByUserId,
            ExpenseCategoryId = dto.ExpenseCategoryId,
            Title = dto.Title.Trim(),
            Description = dto.Description?.Trim(),
            Amount = dto.Amount,
            ExpenseDate = dto.ExpenseDate,
            ExpenseType = dto.ExpenseType
        };
        await expenseRepository.AddAsync(entity, cancellationToken);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    public async Task DeleteExpenseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var expense = await expenseRepository.GetByIdForUpdateAsync(id, cancellationToken);
        if (expense is null) return;

        expenseRepository.Remove(expense);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ExpenseCategoryDto>> GetCategoriesAsync(Guid flatId, CancellationToken cancellationToken = default)
    {
        var categories = await categoryRepository.GetByFlatIdAsync(flatId, cancellationToken);
        return categories.Select(c => new ExpenseCategoryDto { Id = c.Id, Name = c.Name }).ToList();
    }

    private static ExpenseDto MapToDto(Expense expense) => new()
    {
        Id = expense.Id,
        FlatId = expense.FlatId,
        PaidByUserId = expense.PaidByUserId,
        ExpenseCategoryId = expense.ExpenseCategoryId,
        Title = expense.Title,
        Description = expense.Description,
        Amount = expense.Amount,
        ExpenseDate = expense.ExpenseDate,
        ExpenseType = expense.ExpenseType,
        CategoryName = expense.Category?.Name ?? string.Empty,
        PaidByName = expense.PaidBy?.FullName ?? string.Empty
    };
}

using FluentValidation;
using Messly.Application.Common;
using Messly.Application.DTOs;
using Messly.Application.Interfaces.Persistence;
using Messly.Application.Interfaces.Services;
using Messly.Domain.Entities;

namespace Messly.Application.Services;

public class ExpenseService(
    IExpenseRepository expenseRepository,
    IExpenseCategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    IValidator<ExpenseUpsertDto> validator) : IExpenseService
{
    private static readonly string[] DefaultCategoryNames =
        ["Grocery", "Utility", "Gas", "Cook Salary", "Other"];

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
        if (dto.Id.HasValue)
        {
            await UpdateExpenseAsync(dto, cancellationToken);
            return dto.Id.Value;
        }

        return await CreateExpenseAsync(dto, cancellationToken);
    }

    public async Task<Guid> CreateExpenseAsync(ExpenseUpsertDto dto, CancellationToken cancellationToken = default)
    {
        if (dto.Id.HasValue)
            throw new BusinessException("Cannot create an expense with an existing id. Use update instead.");

        await ValidateAsync(dto, cancellationToken);
        await EnsureDefaultCategoriesAsync(dto.FlatId, cancellationToken);
        await ValidateCategoryBelongsToFlatAsync(dto.FlatId, dto.ExpenseCategoryId, cancellationToken);

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

    public async Task UpdateExpenseAsync(ExpenseUpsertDto dto, CancellationToken cancellationToken = default)
    {
        if (!dto.Id.HasValue)
            throw new BusinessException("Expense id is required for update.");

        await ValidateAsync(dto, cancellationToken);
        await EnsureDefaultCategoriesAsync(dto.FlatId, cancellationToken);
        await ValidateCategoryBelongsToFlatAsync(dto.FlatId, dto.ExpenseCategoryId, cancellationToken);

        var expense = await expenseRepository.GetByIdForUpdateAsync(dto.Id.Value, cancellationToken)
            ?? throw new BusinessException("Expense not found.");

        if (expense.FlatId != dto.FlatId)
            throw new BusinessException("Expense does not belong to this flat.");

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
    }

    public async Task DeleteExpenseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var expense = await expenseRepository.GetByIdForUpdateAsync(id, cancellationToken)
            ?? throw new BusinessException("Expense not found.");

        expenseRepository.Remove(expense);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ExpenseCategoryDto>> GetCategoriesAsync(Guid flatId, CancellationToken cancellationToken = default)
    {
        await EnsureDefaultCategoriesAsync(flatId, cancellationToken);
        var categories = await categoryRepository.GetByFlatIdAsync(flatId, cancellationToken);
        return categories.Select(c => new ExpenseCategoryDto { Id = c.Id, Name = c.Name }).ToList();
    }

    private async Task EnsureDefaultCategoriesAsync(Guid flatId, CancellationToken cancellationToken)
    {
        var categories = await categoryRepository.GetByFlatIdAsync(flatId, cancellationToken);
        if (categories.Count > 0)
            return;

        foreach (var name in DefaultCategoryNames)
        {
            await categoryRepository.AddAsync(new ExpenseCategory
            {
                FlatId = flatId,
                Name = name
            }, cancellationToken);
        }

        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task ValidateCategoryBelongsToFlatAsync(Guid flatId, Guid categoryId, CancellationToken cancellationToken)
    {
        var categories = await categoryRepository.GetByFlatIdAsync(flatId, cancellationToken);
        if (categories.All(c => c.Id != categoryId))
            throw new BusinessException("Invalid expense category for this flat.");
    }

    private async Task ValidateAsync(ExpenseUpsertDto dto, CancellationToken cancellationToken)
    {
        var validation = await validator.ValidateAsync(dto, cancellationToken);
        if (!validation.IsValid)
            throw new BusinessException(string.Join(" ", validation.Errors.Select(e => e.ErrorMessage)));
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

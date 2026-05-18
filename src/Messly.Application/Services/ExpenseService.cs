using FluentValidation;
using Messly.Application.Common;
using Messly.Application.DTOs;
using Messly.Application.Interfaces.Persistence;
using Messly.Application.Interfaces.Security;
using Messly.Application.Interfaces.Services;
using Messly.Domain.Entities;

namespace Messly.Application.Services;

public class ExpenseService(
    IExpenseRepository expenseRepository,
    IExpenseCategoryRepository categoryRepository,
    IUnitOfWork unitOfWork,
    IValidator<ExpenseUpsertDto> validator,
    IFlatAuthorizationService authorization) : IExpenseService
{
    private static readonly string[] DefaultCategoryNames =
        ["Grocery", "Utility", "Gas", "Cook Salary", "Other"];

    public async Task<IReadOnlyList<ExpenseDto>> GetExpensesAsync(CancellationToken cancellationToken = default)
    {
        authorization.EnsureCanRead();
        var flatId = authorization.GetCurrentFlatId();
        var expenses = await expenseRepository.GetByFlatIdAsync(flatId, cancellationToken);
        return expenses.Select(MapToDto).ToList();
    }

    public async Task<ExpenseDto?> GetExpenseAsync(Guid id, CancellationToken cancellationToken = default)
    {
        authorization.EnsureCanRead();
        var flatId = authorization.GetCurrentFlatId();
        var expense = await expenseRepository.GetByIdAndFlatAsync(id, flatId, cancellationToken);
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
        authorization.EnsureManager();
        var flatId = authorization.GetCurrentFlatId();

        if (dto.Id.HasValue)
            throw new BusinessException("Cannot create an expense with an existing id. Use update instead.");

        await ValidateAsync(dto, cancellationToken);
        await authorization.EnsureUserIsActiveMemberAsync(dto.PaidByUserId, cancellationToken);
        await EnsureDefaultCategoriesAsync(flatId, cancellationToken);
        await ValidateCategoryBelongsToFlatAsync(flatId, dto.ExpenseCategoryId, cancellationToken);

        var entity = new Expense
        {
            FlatId = flatId,
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
        authorization.EnsureManager();
        var flatId = authorization.GetCurrentFlatId();

        if (!dto.Id.HasValue)
            throw new BusinessException("Expense id is required for update.");

        await ValidateAsync(dto, cancellationToken);
        await authorization.EnsureUserIsActiveMemberAsync(dto.PaidByUserId, cancellationToken);
        await EnsureDefaultCategoriesAsync(flatId, cancellationToken);
        await ValidateCategoryBelongsToFlatAsync(flatId, dto.ExpenseCategoryId, cancellationToken);

        var expense = await expenseRepository.GetByIdForUpdateAndFlatAsync(dto.Id.Value, flatId, cancellationToken)
            ?? throw new NotFoundException();

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
        authorization.EnsureManager();
        var flatId = authorization.GetCurrentFlatId();

        var expense = await expenseRepository.GetByIdForUpdateAndFlatAsync(id, flatId, cancellationToken)
            ?? throw new NotFoundException();

        expenseRepository.Remove(expense);
        await unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ExpenseCategoryDto>> GetCategoriesAsync(CancellationToken cancellationToken = default)
    {
        authorization.EnsureCanRead();
        var flatId = authorization.GetCurrentFlatId();
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

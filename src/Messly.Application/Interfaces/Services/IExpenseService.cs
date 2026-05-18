using Messly.Application.DTOs;

namespace Messly.Application.Interfaces.Services;

public interface IExpenseService
{
    Task<IReadOnlyList<ExpenseDto>> GetExpensesAsync(Guid flatId, CancellationToken cancellationToken = default);
    Task<ExpenseDto?> GetExpenseAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Guid> SaveExpenseAsync(ExpenseUpsertDto dto, CancellationToken cancellationToken = default);
    Task DeleteExpenseAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<ExpenseCategoryDto>> GetCategoriesAsync(Guid flatId, CancellationToken cancellationToken = default);
}

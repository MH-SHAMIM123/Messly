using FluentValidation;
using Messly.Application.DTOs;

namespace Messly.Application.Validators;

public class ExpenseUpsertDtoValidator : AbstractValidator<ExpenseUpsertDto>
{
    public ExpenseUpsertDtoValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.FlatId).NotEmpty();
        RuleFor(x => x.PaidByUserId).NotEmpty();
        RuleFor(x => x.ExpenseCategoryId).NotEmpty();
    }
}

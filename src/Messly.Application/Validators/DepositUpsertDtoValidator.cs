using FluentValidation;
using Messly.Application.DTOs;

namespace Messly.Application.Validators;

public class DepositUpsertDtoValidator : AbstractValidator<DepositUpsertDto>
{
    public DepositUpsertDtoValidator()
    {
        RuleFor(x => x.Amount).GreaterThan(0);
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.FlatId).NotEmpty();
    }
}

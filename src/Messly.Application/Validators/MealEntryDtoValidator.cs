using FluentValidation;
using Messly.Application.DTOs;

namespace Messly.Application.Validators;

public class MealEntryDtoValidator : AbstractValidator<MealEntryDto>
{
    public MealEntryDtoValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.BreakfastCount).InclusiveBetween(0, 3);
        RuleFor(x => x.LunchCount).InclusiveBetween(0, 3);
        RuleFor(x => x.DinnerCount).InclusiveBetween(0, 3);
    }
}

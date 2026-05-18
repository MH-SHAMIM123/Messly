using FluentValidation;
using Messly.Application.DTOs;

namespace Messly.Application.Validators;

public class MemberUpsertDtoValidator : AbstractValidator<MemberUpsertDto>
{
    public MemberUpsertDtoValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.FlatId).NotEmpty();
    }
}

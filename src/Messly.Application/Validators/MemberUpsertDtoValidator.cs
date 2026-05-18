using FluentValidation;
using Messly.Application.DTOs;
using Messly.Domain.Enums;

namespace Messly.Application.Validators;

public class MemberUpsertDtoValidator : AbstractValidator<MemberUpsertDto>
{
    public MemberUpsertDtoValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Phone).MaximumLength(20).When(x => !string.IsNullOrWhiteSpace(x.Phone));
        RuleFor(x => x.RoleType).IsInEnum();
    }
}

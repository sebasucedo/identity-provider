using FluentValidation;

namespace identity_provider.api.validators;

public class ConfirmSignupRequestValidator : AbstractValidator<ConfirmSignupRequest>
{
    public ConfirmSignupRequestValidator()
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage("Username is required");
        RuleFor(x => x.Code).NotEmpty().WithMessage("Code is required");
    }
}

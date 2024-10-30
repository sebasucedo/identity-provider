using FluentValidation;
using identity_provider.api.services;

namespace identity_provider.api.validators;

public class NewPasswordRequestValidator : AbstractValidator<NewPasswordRequest>
{

    public NewPasswordRequestValidator(AdministrationService administrationService)
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage("Username is required");
        RuleFor(x => x.Session).NotEmpty().WithMessage("Session is required");

        var passwordPolicy = administrationService.GetUserPoolPasswordPolicy().Result;
        RuleFor(x => x.NewPassword).ApplyPasswordPolicy(passwordPolicy);
    }
}

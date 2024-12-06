using FluentValidation;
using identity_provider.api.services;

namespace identity_provider.api.validators;

public class PasswordRecoveryRequestValidator : AbstractValidator<PasswordRecoveryRequest>
{
    public PasswordRecoveryRequestValidator(AdministrationService administrationService)
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage("Username is required");
        RuleFor(x => x.ConfirmationCode).NotEmpty().WithMessage("Confirmation code is required");

        RuleFor(x => x.NewPassword).NotEmpty().WithMessage("New password is required");
        RuleFor(x => x.ConfirmationPassword).NotEmpty().WithMessage("Confirmation password is required")
                                            .Equal(x => x.NewPassword).WithMessage("Passwords do not match");
        var passwordPolicy = administrationService.GetUserPoolPasswordPolicy().Result;
        RuleFor(x => x.NewPassword).ApplyPasswordPolicy(passwordPolicy);
    }
}

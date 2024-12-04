using FluentValidation;
using identity_provider.api.services;

namespace identity_provider.api.validators;

public class ChangePasswordRequestValidator : AbstractValidator<ChangePasswordRequest>
{
    public ChangePasswordRequestValidator(AdministrationService administrationService)
    {
        RuleFor(x => x.OldPassword).NotEmpty().WithMessage("Current password is required");
        RuleFor(x => x.NewPassword).NotEmpty().WithMessage("New password is required")
                                   .NotEqual(x => x.OldPassword).WithMessage("New password must not be the same as the previous one");
        RuleFor(x => x.ConfirmationPassword).NotEmpty().WithMessage("Confirmation password is required")
                                            .Equal(x => x.NewPassword).WithMessage("Passwords do not match");

        var passwordPolicy = administrationService.GetUserPoolPasswordPolicy().Result;
        RuleFor(x => x.NewPassword).ApplyPasswordPolicy(passwordPolicy);
    }
}

using FluentValidation;
using identity_provider.api.models;
using identity_provider.api.services;

namespace identity_provider.api.validators;

public class ResetPasswordModelValidator : AbstractValidator<ResetPasswordModel>
{
    public ResetPasswordModelValidator(AdministrationService administrationService)
    {
        RuleFor(x => x.UserId).NotEmpty().WithMessage("UserId is required");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required");

        var passwordPolicy = administrationService.GetUserPoolPasswordPolicy().Result;
        RuleFor(x => x.Password).ApplyPasswordPolicy(passwordPolicy);
    }
}

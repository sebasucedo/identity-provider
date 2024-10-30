using FluentValidation;
using identity_provider.api.services;

namespace identity_provider.api.validators;

public class SignupRequestValidator : AbstractValidator<SignupRequest>
{
    public SignupRequestValidator(AdministrationService administrationService)
    {
        RuleFor(x => x.Username).NotEmpty().WithMessage("Username is required");
        RuleFor(x => x.Email).NotEmpty().WithMessage("Email is required");
        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email is invalid")
            .MustAsync(async (email, cancellation) =>
            {
                var emailExists = await administrationService.EmailExists(email);
                return !emailExists;
            }).WithMessage("Email is already in use");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Password is required");
        RuleFor(x => x.ConfirmationPassword).NotEmpty().WithMessage("Confirmation password is required")
                                            .Equal(x => x.Password).WithMessage("Passwords do not match");

        var passwordPolicy = administrationService.GetUserPoolPasswordPolicy().Result;
        RuleFor(x => x.Password).ApplyPasswordPolicy(passwordPolicy);
    }
}
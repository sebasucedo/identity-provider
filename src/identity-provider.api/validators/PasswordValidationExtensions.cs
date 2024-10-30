using FluentValidation;

namespace identity_provider.api.validators;

public static class PasswordValidationExtensions
{
    public static IRuleBuilderOptions<T, string> ApplyPasswordPolicy<T>(this IRuleBuilder<T, string> ruleBuilder, PasswordPolicy passwordPolicy)
    {
        var rule = ruleBuilder
                    .NotEmpty().WithMessage("Password is required.")
                    .MinimumLength(passwordPolicy.MinimumLength).WithMessage($"Password must be at least {passwordPolicy.MinimumLength} characters long.");

        if (passwordPolicy.RequireNumbers)
            rule = rule.Matches("[0-9]").WithMessage("Password must contain at least one numeric character.");

        if (passwordPolicy.RequireLowercase)
            rule = rule.Matches("[a-z]").WithMessage("Password must contain at least one lowercase letter.");

        if (passwordPolicy.RequireUppercase)
            rule = rule.Matches("[A-Z]").WithMessage("Password must contain at least one uppercase letter.");

        if (passwordPolicy.RequireSymbols)
            rule = rule.Matches("[^a-zA-Z0-9]").WithMessage("Password must contain at least one special character.");

        return rule;
    }
}

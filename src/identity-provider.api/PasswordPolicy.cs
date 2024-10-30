namespace identity_provider.api;

public class PasswordPolicy
{
    public int MinimumLength { get; set; }
    public bool RequireLowercase { get; set; }
    public bool RequireNumbers { get; set; }
    public bool RequireSymbols { get; set; }
    public bool RequireUppercase { get; set; }
    public int TemporaryPasswordValidityDays { get; set; }
}
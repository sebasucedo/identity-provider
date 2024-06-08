namespace identity_provider.api.requests;

public class NewPasswordRequest
{
    public required string Username { get; set; }
    public required string NewPassword { get; set; }
    public required string Session { get; set; }
}

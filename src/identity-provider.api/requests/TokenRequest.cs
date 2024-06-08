namespace identity_provider.api.requests;

public class TokenRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}
using System.Text.Json.Serialization;

namespace identity_provider.api.models;

public class ResetPasswordModel
{
    [JsonIgnore]
    public string UserId { get; set; } = string.Empty;
    public required string Password { get; set; }
}

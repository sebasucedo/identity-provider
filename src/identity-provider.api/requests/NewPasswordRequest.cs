namespace identity_provider.api.requests;

public record NewPasswordRequest(string Username, string NewPassword, string Session);

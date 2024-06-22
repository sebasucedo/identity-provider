namespace identity_provider.api.requests;

public record CreateUserRequest(string Username, string TemporaryPassword);
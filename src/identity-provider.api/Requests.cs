namespace identity_provider.api;

public record CreateUserRequest(string Username, string Email, string TemporaryPassword);
public record NewPasswordRequest(string Username, string NewPassword, string Session);
public record ResetPasswordRequest(string Username, string Password);
public record SignupRequest(string Username, string Email, string Password, string ConfirmationPassword);
public record ConfirmSignupRequest(string Username, string Code);
public record TokenRequest(string Username, string Password);
public record ResendConfirmationRequest(string Username);
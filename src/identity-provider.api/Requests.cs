namespace identity_provider.api;

public record CreateUserRequest(string Username, string Email, string TemporaryPassword);
public record NewPasswordRequest(string Username, string NewPassword, string Session);
public record ChangePasswordRequest(string AccessToken, string OldPassword, string NewPassword, string ConfirmationPassword);
public record SignupRequest(string Username, string Email, string Password, string ConfirmationPassword);
public record ConfirmSignupRequest(string Username, string Code);
public record TokenRequest(string Username, string Password);
public record RefreshTokenRequest(string Username, string RefreshToken);
public record ResendConfirmationRequest(string Username);
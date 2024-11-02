namespace identity_provider.api;

public record UserCreatedResponse(string UserId, string Status, string Message);

public record AuthenticationResponse(string? AccessToken,
                                     string? IdToken,
                                     string? RefreshToken,
                                     string? ChallengeName,
                                     string? Session);

public record SignUpResponse(string UserId, string CodeDeliveryDetails);
namespace identity_provider.api;

public class AwsConfig
{
    public required string Region { get; set; }

    public required CognitoConfig Cognito { get; set; }

    public class CognitoConfig
    {
        public required string UserPoolId { get; set; }
        public required string ClientId { get; set; }
        public required string ClientSecret { get; set; }
    }
}
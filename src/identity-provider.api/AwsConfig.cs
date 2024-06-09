namespace identity_provider.api;

public class AwsConfig
{
    public required string Region { get; set; }
    public required string AccessKey { get; set; }
    public required string SecretKey { get; set; }

    public required CognitoConfig Cognito { get; set; }
    public required CloudWatchConfig CloudWatch { get; set; }

    public class CognitoConfig
    {
        public required string UserPoolId { get; set; }
        public required string ClientId { get; set; }
        public required string ClientSecret { get; set; }
    }
    
    public class CloudWatchConfig
    {
        public required string LogGroupName { get; set; }
        public required string LogStreamName { get; set; }
    }
}
namespace identity_provider.api;

public static class Constants
{
    public static class EnvironmentVariables
    {
        public const string AWS_REGION = "REGION";
        public const string AWS_ACCESS_KEY = "ACCESS_KEY";
        public const string AWS_SECRET_KEY = "SECRET_KEY";
        public const string AWS_SECRET_NAME = "SECRET_NAME";
    }

    public static class Endpoints
    {
        public const string TOKEN = "/token";
        public const string NEW_PASSWORD = "/new-password";
        public const string CREATE_USER = "/create-user";
        public const string PROFILE = "/profile";
    }
    public static class Credentials
    {
        public const string USERNAME = "USERNAME";
        public const string PASSWORD = "PASSWORD";
        public const string SECRET_HASH = "SECRET_HASH";
    }
}

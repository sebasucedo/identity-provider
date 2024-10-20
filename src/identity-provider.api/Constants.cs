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
        public const string PROFILE = "/profile";
        public const string HEALTHCHECK = "/healthcheck";

        public const string CREATE_USER = "/admin/create-user";
        public const string RESET_PASSWORD = "/admin/reset-password";
    }

    public static class Credentials
    {
        public const string USERNAME = "USERNAME";
        public const string PASSWORD = "PASSWORD";
        public const string SECRET_HASH = "SECRET_HASH";
    }

    public static class SecretsManager
    {
        public const string AWSCURRENT = "AWSCURRENT";
    }

    public static class Policies
    {
        public const string CORS_POLICY = "CORSPolicy";
        public const string AUTHENTICATED_POLICY = "Authenticated";
        public const string ADMINISTRATORS_ONLY_POLICY = "AdministratorsOnly";
    }

    public static class Roles
    {
        public const string ADMINISTRATORS = "administrators";
    }

    public static class FormIdentifier
    {
        public const string TOKEN_USERNAME = "username";
        public const string TOKEN_PASSWORD = "password";

        public const string NEW_PASSWORD_USERNAME = "username";
        public const string NEW_PASSWORD_NEW_PASSWORD = "newPassword";
        public const string NEW_PASSWORD_SESSION = "session";
    }

    public static class Cognito
    {
        public static string USERNAME_CLAIM { get; } = "cognito:username";
        public static string ROLE_CLAIM { get; } = "cognito:groups";
        public static string EMAIL_CLAIM { get; } = "email";
        public static string SUB_CLAIM { get; } = "sub";
    }
    public static class Tags
    {
        public const string ADMIN = "Admin";
    }

    public static class Keys
    {
        public const string APP_CLIENT = "AppClient";
        public const string IAM_CLIENT = "IamClient";
    }

}

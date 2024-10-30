using Amazon.CognitoIdentityProvider.Model;
using Amazon.CognitoIdentityProvider;
using Microsoft.Extensions.Options;

namespace identity_provider.api.services;

public class AdministrationService(Func<string, IAmazonCognitoIdentityProvider> cognitoClientFactory, 
                                   IOptions<AwsConfig> awsConfigOptions)
{
    private readonly IAmazonCognitoIdentityProvider _adminProvider = cognitoClientFactory(Constants.Keys.IAM_CLIENT);
    private readonly string _userPoolId = awsConfigOptions.Value.Cognito.UserPoolId;

    public async Task<ResetPasswordResponse> ResetPassword(string username, string password)
    {
        var resetPasswordRequest = new AdminSetUserPasswordRequest
        {
            UserPoolId = _userPoolId,
            Username = username,
            Password = password,
            Permanent = true,
        };

        var response = await _adminProvider.AdminSetUserPasswordAsync(resetPasswordRequest);
        var user = await GetUser(username);

        var result = new ResetPasswordResponse(user.UserAttributes.Single(a => a.Name == Constants.Cognito.SUB_CLAIM).Value, 
                                               "Password updated successfully");
        return result;
    }

    public async Task<UserCreatedResponse> CreateUser(string username, string email, string temporaryPassword)
    {
        var adminCreateUserRequest = new AdminCreateUserRequest
        {
            UserPoolId = _userPoolId,
            Username = username,
            TemporaryPassword = temporaryPassword,
            MessageAction = "SUPPRESS",
            UserAttributes =
                [
                    new AttributeType
                    {
                        Name = Constants.AttributeTypes.EMAIL,
                        Value = email
                    },
                    new AttributeType
                    {
                        Name = Constants.AttributeTypes.EMAIL_VERIFIED,
                        Value = "True",
                    }
                ]
        };

        //https://docs.aws.amazon.com/cognito-user-identity-pools/latest/APIReference/API_AdminCreateUser.html
        var response = await _adminProvider.AdminCreateUserAsync(adminCreateUserRequest);
        var result = new UserCreatedResponse(response.User.Attributes.Single(a => a.Name == Constants.Cognito.SUB_CLAIM).Value, 
                                             response.User.UserStatus, 
                                             "User created successfully");
        return result;
    }

    private async Task<AdminGetUserResponse> GetUser(string username)
    {
        var request = new AdminGetUserRequest
        {
            UserPoolId = _userPoolId,
            Username = username,
        };

        var response = await _adminProvider.AdminGetUserAsync(request);
        return response;
    }

    public async Task<bool> EmailExists(string email)
    {
        var request = new ListUsersRequest
        {
            UserPoolId = _userPoolId,
            Filter = $"email = \"{email}\""
        };

        var response = await _adminProvider.ListUsersAsync(request);
        return response.Users.Count != 0;
    }

    public async Task<PasswordPolicy> GetUserPoolPasswordPolicy()
    {
        var request = new DescribeUserPoolRequest
        {
            UserPoolId = _userPoolId
        };

        var response = await _adminProvider.DescribeUserPoolAsync(request);
        var passwordPolicy = response.UserPool.Policies.PasswordPolicy;

        return new PasswordPolicy
        {
            MinimumLength = passwordPolicy.MinimumLength,
            RequireLowercase = passwordPolicy.RequireLowercase,
            RequireNumbers = passwordPolicy.RequireNumbers,
            RequireSymbols = passwordPolicy.RequireSymbols,
            RequireUppercase = passwordPolicy.RequireUppercase,
            TemporaryPasswordValidityDays = passwordPolicy.TemporaryPasswordValidityDays,
        };
    }
}

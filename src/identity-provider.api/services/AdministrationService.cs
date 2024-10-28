﻿using Amazon.CognitoIdentityProvider.Model;
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
                        Name = "email",
                        Value = email
                    },
                    new AttributeType
                    {
                        Name = "email_verified",
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

}

public record ResetPasswordResponse(string UserId, string Message);
public record UserCreatedResponse(string UserId, string Status, string Message);
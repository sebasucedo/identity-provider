
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace identity_provider.api.services;

public class AuthenticationService(IAmazonCognitoIdentityProvider amazonCognitoIdentityProvider,
                                   IOptions<AwsConfig> awsConfigOptions)
{
    private readonly IAmazonCognitoIdentityProvider _provider = amazonCognitoIdentityProvider;
    private readonly string _userPoolId = awsConfigOptions.Value.Cognito.UserPoolId;
    private readonly string _clientId = awsConfigOptions.Value.Cognito.ClientId;
    private readonly string _clientSecret = awsConfigOptions.Value.Cognito.ClientSecret;

    public async Task<AuthenticationResponse> Authenticate(string username,
                                                           string password)
    {
        var authRequest = new InitiateAuthRequest
        {
            AuthFlow = AuthFlowType.USER_PASSWORD_AUTH,
            ClientId = _clientId,
            AuthParameters =
            {
                { "USERNAME", username },
                { "PASSWORD", password },
            }
        };

        if (!string.IsNullOrEmpty(_clientSecret))
            authRequest.AuthParameters.Add("SECRET_HASH", CalculateSecretHash(username));

        var authResponse = await _provider.InitiateAuthAsync(authRequest);

        if (authResponse.ChallengeName == ChallengeNameType.NEW_PASSWORD_REQUIRED)
            return new AuthenticationResponse(null,
                                              null,
                                              null,
                                              authResponse.ChallengeName,
                                              authResponse.Session);

        var result = new AuthenticationResponse(authResponse.AuthenticationResult.AccessToken,
                                                authResponse.AuthenticationResult.IdToken,
                                                authResponse.AuthenticationResult.RefreshToken,
                                                null,
                                                null);

        return result;
    }

    public async Task<AuthenticationResponse> RespondToNewPasswordChallenge(string username,
                                                                            string newPassword,
                                                                            string session)
    {
        var secretHash = CalculateSecretHash(username);

        var respondToAuthChallengeRequest = new RespondToAuthChallengeRequest
        {
            ClientId = _clientId,
            ChallengeName = ChallengeNameType.NEW_PASSWORD_REQUIRED,
            Session = session,
            ChallengeResponses = new Dictionary<string, string>
            {
                { "USERNAME", username },
                { "NEW_PASSWORD", newPassword },
                { "SECRET_HASH", secretHash }
            }
        };

        var authResponse = await _provider.RespondToAuthChallengeAsync(respondToAuthChallengeRequest);

        var result = new AuthenticationResponse(authResponse.AuthenticationResult.AccessToken,
                                                authResponse.AuthenticationResult.IdToken,
                                                authResponse.AuthenticationResult.RefreshToken,
                                                null,
                                                null);

        return result;
    }

    public async Task CreateUser(string username, string temporaryPassword)
    {
        var adminCreateUserRequest = new AdminCreateUserRequest
        {
            UserPoolId = _userPoolId,
            Username = username,
            TemporaryPassword = temporaryPassword,
            MessageAction = "SUPPRESS"
        };

        //https://docs.aws.amazon.com/cognito-user-identity-pools/latest/APIReference/API_AdminCreateUser.html
        var response = await _provider.AdminCreateUserAsync(adminCreateUserRequest);

        return;
    }

    private string CalculateSecretHash(string username)
    {
        byte[] secretBytes = Encoding.UTF8.GetBytes(_clientSecret);
        byte[] messageBytes = Encoding.UTF8.GetBytes(username + _clientId);

        using var hmac = new HMACSHA256(secretBytes);
        byte[] hashBytes = hmac.ComputeHash(messageBytes);
        return Convert.ToBase64String(hashBytes);
    }
}

public record AuthenticationResponse(string? AccessToken,
                                     string? IdToken,
                                     string? RefreshToken,
                                     string? ChallengeName,
                                     string? Session);

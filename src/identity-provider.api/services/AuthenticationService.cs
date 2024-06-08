
using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Amazon.Runtime.Internal;
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

    public async Task<AuthenticationResponse> Authenticate(string username, string password)
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
        {
            return new AuthenticationResponse
            {
                ChallengeName = authResponse.ChallengeName,
                Session = authResponse.Session,
            };
        }

        var result = new AuthenticationResponse
        {
            AccessToken = authResponse.AuthenticationResult.AccessToken,
            IdToken = authResponse.AuthenticationResult.IdToken,
            RefreshToken = authResponse.AuthenticationResult.RefreshToken,
        };

        return result;
    }

    public async Task<AuthenticationResponse> RespondToNewPasswordChallenge(string username, string newPassword, string session)
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

        var result = new AuthenticationResponse
        {
            AccessToken = authResponse.AuthenticationResult.AccessToken,
            IdToken = authResponse.AuthenticationResult.IdToken,
            RefreshToken = authResponse.AuthenticationResult.RefreshToken,
        };

        return result;
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

public class AuthenticationResponse
{
    public string? AccessToken { get; set; }
    public string? IdToken { get; set; }
    public string? RefreshToken { get; set; }
    public string? ChallengeName { get; set; }
    public string? Session { get; set; }
}
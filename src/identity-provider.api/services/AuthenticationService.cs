using Amazon.CognitoIdentityProvider;
using Amazon.CognitoIdentityProvider.Model;
using Microsoft.Extensions.Options;
using System.Net;
using System.Security.Cryptography;
using System.Text;

namespace identity_provider.api.services;

public class AuthenticationService(Func<string, IAmazonCognitoIdentityProvider> cognitoClientFactory,
                                   IOptions<AwsConfig> awsConfigOptions)
{
    private readonly IAmazonCognitoIdentityProvider _provider = cognitoClientFactory(Constants.Keys.APP_CLIENT);
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
                { Constants.Keys.USERNAME, username },
                { Constants.Keys.PASSWORD, password },
            }
        };

        if (!string.IsNullOrEmpty(_clientSecret))
            authRequest.AuthParameters.Add(Constants.Keys.SECRET_HASH, CalculateSecretHash(username));

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
                { Constants.Keys.USERNAME, username },
                { Constants.Keys.NEW_PASSWORD, newPassword },
                { Constants.Keys.SECRET_HASH, secretHash }
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

    public async Task<SignUpResponse> SignUp(string username,
                                             string email,
                                             string password)
    {
        var signUpRequest = new SignUpRequest
        {
            ClientId = _clientId,
            Username = username,
            Password = password,
            UserAttributes =
            {
                new AttributeType
                {
                    Name = Constants.AttributeTypes.EMAIL,
                    Value = email
                }
            }
        };

        if (!string.IsNullOrEmpty(_clientSecret))
            signUpRequest.SecretHash = CalculateSecretHash(username);

        var response = await _provider.SignUpAsync(signUpRequest);
        var codeDeliveryDetails = $"A verification code has been sent to your email ({response.CodeDeliveryDetails.Destination}).";
        var result = new SignUpResponse(response.UserSub, 
                                        codeDeliveryDetails);

        return result;
    }

    public async Task ConfirmSignUp(string username, 
                                    string confirmationCode)
    {
        var request = new ConfirmSignUpRequest
        {
            ClientId = _clientId,
            Username = username,
            ConfirmationCode = confirmationCode
        };

        if (!string.IsNullOrEmpty(_clientSecret))
            request.SecretHash = CalculateSecretHash(username);

        var response = await _provider.ConfirmSignUpAsync(request);

        if (response.HttpStatusCode != HttpStatusCode.OK)
            throw new Exception("Error confirming signup");
    }

    public async Task ResendConfirmation(string username)
    {
        var request = new ResendConfirmationCodeRequest
        {
            ClientId = _clientId,
            Username = username
        };

        if (!string.IsNullOrEmpty(_clientSecret))
            request.SecretHash = CalculateSecretHash(username);

        var response = await _provider.ResendConfirmationCodeAsync(request);

        if (response.HttpStatusCode != HttpStatusCode.OK)
            throw new Exception("Error resending confirmation code");
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


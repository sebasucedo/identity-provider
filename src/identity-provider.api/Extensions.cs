using Amazon;
using Amazon.CognitoIdentityProvider;
using Amazon.Runtime;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace identity_provider.api;

public static class Extensions
{
    public static IServiceCollection AddSecurity(this IServiceCollection services)
    {
        using var serviceProvider = services.BuildServiceProvider();
        var awsConfig = serviceProvider.GetRequiredService<IOptions<AwsConfig>>().Value;

        string validIssuer = $"https://cognito-idp.{awsConfig.Region}.amazonaws.com/{awsConfig.Cognito.UserPoolId}";
        string validAudience = awsConfig.Cognito.ClientId;

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
                {
                    options.Authority = validIssuer;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = validIssuer,
                        ValidateAudience = true,
                        ValidAudience = validAudience,
                        ValidateLifetime = true
                    };
                });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("Authenticated", policy =>
            {
                policy.RequireAuthenticatedUser();
            });
        });

        services.AddTransient<IAmazonCognitoIdentityProvider>(provider =>
        {
            return new AmazonCognitoIdentityProviderClient(
                        new BasicAWSCredentials(awsConfig.Cognito.ClientId, awsConfig.Cognito.ClientSecret),
                        RegionEndpoint.GetBySystemName(awsConfig.Region)
                                                         );
        });

        return services;
    }
}

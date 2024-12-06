using Amazon;
using Amazon.CloudWatchLogs;
using Amazon.CognitoIdentityProvider;
using Amazon.Runtime;
using FluentValidation;
using identity_provider.api.models;
using identity_provider.api.services;
using identity_provider.api.validators;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Sinks.AwsCloudWatch;
using System;

namespace identity_provider.api;

public static class Extensions
{
    public static IServiceCollection AddServices(this IServiceCollection services)
    {
        using var serviceProvider = services.BuildServiceProvider();
        var awsConfig = serviceProvider.GetRequiredService<IOptions<AwsConfig>>().Value;

        services.AddTransient<Func<string, IAmazonCognitoIdentityProvider>>(serviceProvider => key =>
        {
            return key switch
            {
                Constants.Keys.APP_CLIENT => new AmazonCognitoIdentityProviderClient(
                                                new BasicAWSCredentials(awsConfig.Cognito.ClientId, awsConfig.Cognito.ClientSecret),
                                                RegionEndpoint.GetBySystemName(awsConfig.Region)),
                Constants.Keys.IAM_CLIENT => new AmazonCognitoIdentityProviderClient(
                                                new BasicAWSCredentials(awsConfig.AccessKey, awsConfig.SecretKey),
                                                RegionEndpoint.GetBySystemName(awsConfig.Region)),
                _ => throw new ArgumentException("Invalid client key")
            };
        });
        services.AddTransient<AuthenticationService>();
        services.AddTransient<AdministrationService>();

        services.AddScoped<IValidator<SignupRequest>, SignupRequestValidator>();
        services.AddScoped<IValidator<TokenRequest>, TokenRequestValidator>();
        services.AddScoped<IValidator<NewPasswordRequest>, NewPasswordRequestValidator>();
        services.AddScoped<IValidator<ChangePasswordRequest>, ChangePasswordRequestValidator>();
        services.AddScoped<IValidator<ForgotPasswordRequest>, ForgotPasswordRequestValidator>();
        services.AddScoped<IValidator<PasswordRecoveryRequest>, PasswordRecoveryRequestValidator>();
        services.AddScoped<IValidator<ConfirmSignupRequest>, ConfirmSignupRequestValidator>();
        services.AddScoped<IValidator<ResendConfirmationRequest>, ResendConfirmationRequestValidator>();
        services.AddScoped<IValidator<ResetPasswordModel>, ResetPasswordModelValidator>();

        return services;
    }

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
                        ValidateLifetime = true,
                        ValidAudience = validAudience,
                        ValidateAudience = true,

                        ValidateIssuer = true,
                        ValidIssuer = validIssuer,

                        NameClaimType = Constants.Cognito.USERNAME_CLAIM,
                        RoleClaimType = Constants.Cognito.ROLE_CLAIM,
                    };
                });

        services.AddAuthorization(options =>
        {
            options.AddPolicy(Constants.Policies.AUTHENTICATED_POLICY, policy =>
            {
                policy.RequireAuthenticatedUser();
            });
            options.AddPolicy(Constants.Policies.ADMINISTRATORS_ONLY_POLICY, policy =>
            {
                policy.RequireRole(Constants.Roles.ADMINISTRATORS);
            });
        });

        return services;
    }

    public static IServiceCollection AddSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "Identity provider", Version = "v1" });

            c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Description = "JWT Authorization header using the Bearer scheme. Example: 'Bearer {token}'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
            });

            c.AddSecurityDefinition(Constants.Keys.X_CSRF_TOKEN, new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Name = Constants.Keys.X_CSRF_TOKEN,
                Type = SecuritySchemeType.ApiKey,
                Description = "CSRF token required for POST requests"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = Constants.Keys.X_CSRF_TOKEN
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        return services;
    }

    public static IServiceCollection ConfigureLogger(this IServiceCollection services, IConfiguration configuration)
    {
        using var serviceProvider = services.BuildServiceProvider();
        var awsConfig = serviceProvider.GetRequiredService<IOptions<AwsConfig>>().Value;

        var logClient = new AmazonCloudWatchLogsClient(awsConfig.AccessKey, awsConfig.SecretKey, RegionEndpoint.GetBySystemName(awsConfig.Region));

        Log.Logger = new LoggerConfiguration()
                         .ReadFrom.Configuration(configuration)
                         .Enrich.FromLogContext()
                         .WriteTo.AmazonCloudWatch(
                             logGroup: awsConfig.CloudWatch.LogGroupName,
                             logStreamPrefix: DateTime.UtcNow.ToString("yyyyMMddHHmmssfff"),
                             batchSizeLimit: 100,
                             queueSizeLimit: 10000,
                             batchUploadPeriodInSeconds: 15,
                             createLogGroup: true,
                             maxRetryAttempts: 3,
                             logGroupRetentionPolicy: LogGroupRetentionPolicy.OneMonth,
                             cloudWatchClient: logClient,
                             restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Warning)
                         .CreateLogger();

        return services;
    }
}

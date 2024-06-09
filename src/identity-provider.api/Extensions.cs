using Amazon;
using Amazon.CloudWatchLogs;
using Amazon.CognitoIdentityProvider;
using Amazon.Runtime;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Sinks.AwsCloudWatch;

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
        });

        return services;
    }

    public static IServiceCollection ConfigureLogger(this IServiceCollection services, IConfiguration configuration)
    {
        using var serviceProvider = services.BuildServiceProvider();
        var awsConfig = serviceProvider.GetRequiredService<IOptions<AwsConfig>>().Value;

        var logClient = new AmazonCloudWatchLogsClient(awsConfig.AccessKey, awsConfig.SecretKey, awsConfig.Region);

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
                restrictedToMinimumLevel: Serilog.Events.LogEventLevel.Information)
            .CreateLogger();

        
        return services;
    }
}

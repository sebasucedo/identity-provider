using Amazon;
using Amazon.Runtime;
using Amazon.SecretsManager;
using Amazon.SecretsManager.Model;
using System.Text;

namespace identity_provider.api;

public class SecretsManagerHelper
{
    public static async Task<IConfiguration> GetConfigurationFromPlainText()
    {
        string region = Environment.GetEnvironmentVariable("REGION") ?? throw new InvalidOperationException("REGION");
        string accessKey = Environment.GetEnvironmentVariable("ACCESS_KEY") ?? throw new InvalidOperationException("ACCESS_KEY");
        string secretKey = Environment.GetEnvironmentVariable("SECRET_KEY") ?? throw new InvalidOperationException("SECRET_KEY");
        string secretName = Environment.GetEnvironmentVariable("SECRET_NAME") ?? throw new InvalidOperationException("SECRET_NAME");
    
        var credential = new BasicAWSCredentials(accessKey, secretKey);
        using var client = new AmazonSecretsManagerClient(credential, RegionEndpoint.GetBySystemName(region));
        var secretValue = await client.GetSecretValueAsync(new GetSecretValueRequest 
        { 
            SecretId = secretName,
            VersionStage = "AWSCURRENT"
        });

        var secretString = secretValue.SecretString ?? throw new InvalidOperationException(secretName);
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(secretString));
        using var reader = new StreamReader(stream);

        var builder = new ConfigurationBuilder()
                          .SetBasePath(AppContext.BaseDirectory)
                          .AddJsonStream(reader.BaseStream)
                          .AddEnvironmentVariables();

        IConfiguration configuration = builder.Build();

        return configuration;
    }
}

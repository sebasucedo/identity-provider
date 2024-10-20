using identity_provider.api;
using identity_provider.api.endpoints;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddAWSLambdaHosting(LambdaEventSource.RestApi);

IConfigurationRoot configuration = await GetConfiguration(builder);

builder.Services.AddServices();

builder.Services.AddSecurity();

builder.Services.ConfigureLogger(configuration);
builder.Host.UseSerilog();

builder.Services.AddSwagger();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.OperationFilter<FormDataOperationFilter>();
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.AddEndpoints();
app.AddAdminEndpoints();

app.Run();

static async Task<IConfigurationRoot> GetConfiguration(WebApplicationBuilder builder)
{
    IConfigurationRoot configuration;
    if (builder.Environment.IsDevelopment())
        configuration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true)
                        .AddEnvironmentVariables()
                        .Build() ?? throw new Exception("Configuration is null");
    else
        configuration = await SecretsManagerHelper.GetConfigurationFromPlainText();
    builder.Services.Configure<AwsConfig>(configuration.GetSection("Aws"));
    return configuration;
}
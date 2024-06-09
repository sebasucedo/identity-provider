using identity_provider.api;
using identity_provider.api.services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = await SecretsManagerHelper.GetConfigurationFromPlainText();
builder.Services.Configure<AwsConfig>(configuration.GetSection("Aws"));

builder.Services.AddTransient<AuthenticationService>();

builder.Services.AddSecurity();
builder.Services.AddSwagger();

builder.Services.ConfigureLogger(configuration);
builder.Host.UseSerilog();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
    app.UseSwagger();
    app.UseSwaggerUI();
//}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

Endpoints.Map(app);

app.Run();

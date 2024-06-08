using identity_provider.api;
using identity_provider.api.services;

var builder = WebApplication.CreateBuilder(args);

IConfiguration configuration = await SecretsManagerHelper.GetConfigurationFromPlainText();
builder.Services.Configure<AwsConfig>(configuration.GetSection("Aws"));

builder.Services.AddTransient<AuthenticationService>();


builder.Services.AddSecurity();

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

Endpoints.Map(app);

app.Run();

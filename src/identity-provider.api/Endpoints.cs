using identity_provider.api.requests;
using identity_provider.api.services;

namespace identity_provider.api;

public static class Endpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPost("/token", async (HttpContext context, TokenRequest model, AuthenticationService service) =>
        {
            Serilog.Log.Error("Test");

            try
            {
                var result = await service.Authenticate(model.Username, model.Password);
                return Results.Ok(result);
            }
            catch (Amazon.CognitoIdentityProvider.Model.NotAuthorizedException ex)
            {
                var response = new
                {
                    ex.Message,
                };

                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return Results.Json(response);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error getting token");
                throw;
            }
        })
        .WithName("GetToken")
        .WithOpenApi();

        app.MapPost("/new-password", async (HttpContext context, NewPasswordRequest request, AuthenticationService service) =>
        {
            try
            {
                var result = await service.RespondToNewPasswordChallenge(request.Username, request.NewPassword, request.Session);
                return Results.Ok(result);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error setting new password");
                throw;
            }
        })
        .WithName("PostNewPassword")
        .WithOpenApi();

        app.MapPost("/create-user", async (HttpContext context, CreateUserRequest request, AuthenticationService service) =>
        {
            try
            {
                await service.CreateUser(request.Username, request.TemporaryPassword);
                return Results.Ok();
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error creating user");
                throw;
            }
        })
        .WithName("PostCreateUser")
        .WithOpenApi();

        app.MapGet("/profile", (HttpContext context) =>
        {
            var user = context.User;
            var claims = user.Claims.Select(item => new KeyValuePair<string, string>(item.Type, item.Value)).ToList();
            return Results.Ok(claims);
        })
        .WithName("GetProfile")
        .WithOpenApi()
        .RequireAuthorization();
    }
}

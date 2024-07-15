using identity_provider.api.requests;
using identity_provider.api.services;

namespace identity_provider.api;

public static class Endpoints
{
    public static void Map(WebApplication app)
    {
        app.MapPost(Constants.Endpoints.TOKEN, async (HttpContext context, AuthenticationService service) =>
        {
            try
            {
                var validationResponse = await ValidateTokenRequest(context);
                if (validationResponse != null)
                    return Results.Json(validationResponse);

                var form = await context.Request.ReadFormAsync();
                var model = new TokenRequest(
                                    form[Constants.FormIdentifier.TOKEN_USERNAME].ToString(),
                                    form[Constants.FormIdentifier.TOKEN_PASSWORD].ToString()
                                );

                var result = await service.Authenticate(model.Username, model.Password);
                var response = new ApiResponse<AuthenticationResponse>
                {
                    Success = true,
                    Message = "Token generated",
                    Data = result,
                };
                return Results.Ok(response);
            }
            catch (Amazon.CognitoIdentityProvider.Model.NotAuthorizedException ex)
            {
                var response = new ApiResponse<string>
                {
                    Success = false,
                    Message = ex.Message,
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

        app.MapPost(Constants.Endpoints.NEW_PASSWORD, async (HttpContext context, AuthenticationService service) =>
        {
            try
            {
                var validationResponse = await ValidateNewPasswordRequest(context);
                if (validationResponse != null)
                    return Results.Json(validationResponse);

                var form = await context.Request.ReadFormAsync();
                var request = new NewPasswordRequest(
                                    form[Constants.FormIdentifier.NEW_PASSWORD_USERNAME].ToString(),
                                    form[Constants.FormIdentifier.NEW_PASSWORD_NEW_PASSWORD].ToString(),
                                    form[Constants.FormIdentifier.NEW_PASSWORD_SESSION].ToString()
                                );
                var result = await service.RespondToNewPasswordChallenge(request.Username, request.NewPassword, request.Session);
                var response = new ApiResponse<AuthenticationResponse>
                {
                    Success = true,
                    Message = "Password set",
                    Data = result,
                };
                return Results.Ok(response);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error setting new password");
                throw;
            }
        })
        .WithName("PostNewPassword")
        .WithOpenApi();

        app.MapPost(Constants.Endpoints.CREATE_USER, async (HttpContext context, CreateUserRequest request, AuthenticationService service) =>
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
        //.RequireAuthorization();

        app.MapGet(Constants.Endpoints.PROFILE, (HttpContext context) =>
        {
            var user = context.User;
            var claims = user.Claims.Select(item => new KeyValuePair<string, string>(item.Type, item.Value)).ToList();
            var response = new ApiResponse<List<KeyValuePair<string, string>>>
            {
                Success = true,
                Message = "Profile retrieved",
                Data = claims,
            };
            return Results.Ok(response);
        })
        .WithName("GetProfile")
        .WithOpenApi()
        .RequireAuthorization();

        app.MapGet(Constants.Endpoints.HEALTHCHECK, () =>
        {
            return Results.Ok("Healthy");
        });
    }

    private static async Task<ApiResponse<string>?> ValidateTokenRequest(HttpContext context)
    {
        if (!context.Request.HasFormContentType)
        {
            var response = new ApiResponse<string>
            {
                Success = false,
                Message = "Invalid content type",
            };
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return response;
        }

        var form = await context.Request.ReadFormAsync();
        if (!form.ContainsKey(Constants.FormIdentifier.TOKEN_USERNAME) || !form.ContainsKey(Constants.FormIdentifier.TOKEN_PASSWORD))
        {
            var response = new ApiResponse<string>
            {
                Success = false,
                Message = "Missing username or password",
            };
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return response;
        }

        return null;
    }

    private static async Task<ApiResponse<string>?> ValidateNewPasswordRequest(HttpContext context)
    {
        if (!context.Request.HasFormContentType)
        {
            var response = new ApiResponse<string>
            {
                Success = false,
                Message = "Invalid content type",
            };
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return response;
        }

        var form = await context.Request.ReadFormAsync();
        if (!form.ContainsKey(Constants.FormIdentifier.NEW_PASSWORD_USERNAME) ||
            !form.ContainsKey(Constants.FormIdentifier.NEW_PASSWORD_NEW_PASSWORD) ||
            !form.ContainsKey(Constants.FormIdentifier.NEW_PASSWORD_SESSION))
        {
            var response = new ApiResponse<string>
            {
                Success = false,
                Message = "Missing username, newPassword or session",
            };
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return response;
        }

        return null;

    }
}
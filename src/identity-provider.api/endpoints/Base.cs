using Amazon.CognitoIdentityProvider.Model;
using FluentValidation;
using identity_provider.api.services;
using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;

namespace identity_provider.api.endpoints;

public static class Base
{
    public static void AddEndpoints(this WebApplication app)
    {
        app.MapPost(Constants.Endpoints.TOKEN, async (IValidator<TokenRequest> validator, [FromForm] TokenRequest request, AuthenticationService service) =>
        {
            try
            {
                var validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                    return Results.ValidationProblem(validationResult.ToDictionary());

                var result = await service.Authenticate(request.Username, request.Password);
                var response = new ApiResponse<AuthenticationResponse>
                {
                    Success = true,
                    Message = "Tokens generated",
                    Data = result,
                };
                return Results.Ok(response);
            }
            catch (NotAuthorizedException ex)
            {
                var errorResponse = new ApiResponse<string>
                {
                    Success = false,
                    Message = ex.Message,
                };
                return Results.Json(errorResponse, statusCode: StatusCodes.Status401Unauthorized);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error getting tokens");
                throw;
            }
        })
        .WithName("PostToken")
        .WithTags(Constants.Tags.BASE)
        .WithOpenApi();

        app.MapPost(Constants.Endpoints.REFRESH_TOKEN, async(RefreshTokenRequest request, AuthenticationService service) =>
        {
            try
            {
                var result = await service.RefreshToken(request.Username, request.RefreshToken);
                var response = new ApiResponse<AuthenticationResponse>
                {
                    Success = true,
                    Message = "Tokens generated",
                    Data = result,
                };
                return Results.Ok(response);
            }
            catch (NotAuthorizedException ex)
            {
                var errorResponse = new ApiResponse<string>
                {
                    Success = false,
                    Message = ex.Message,
                };
                return Results.Json(errorResponse, statusCode: StatusCodes.Status401Unauthorized);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error getting tokens");
                throw;
            }
        })
        .WithName("PostRefreshToken")
        .WithTags(Constants.Tags.BASE)
        .WithOpenApi();

        app.MapPost(Constants.Endpoints.NEW_PASSWORD, async (IValidator<NewPasswordRequest> validator, [FromForm] NewPasswordRequest request, AuthenticationService service) =>
        {
            try
            {
                var validationResult = await validator.ValidateAsync(request);
                if (!validationResult.IsValid)
                    return Results.ValidationProblem(validationResult.ToDictionary());

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
        .WithTags(Constants.Tags.BASE)
        .WithOpenApi();

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
        .WithTags(Constants.Tags.BASE)
        .WithOpenApi()
        .RequireAuthorization();

        app.MapGet(Constants.Endpoints.GENERATE_ANTIFORGERY_TOKEN, (HttpContext httpContext, IAntiforgery antiforgery) =>
        {
            var tokens = antiforgery.GetAndStoreTokens(httpContext);
            httpContext.Response.Cookies.Append(Constants.Keys.X_CSRF_TOKEN, tokens.RequestToken!, new CookieOptions { HttpOnly = false });
            return Results.Ok(new { token = tokens.RequestToken });
        })
        .WithName("GetGenerateAntiforgeryToken")
        .WithTags(Constants.Tags.BASE)
        .WithOpenApi();

    }
}

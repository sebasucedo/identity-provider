using Amazon.CognitoIdentityProvider.Model;
using identity_provider.api.requests;
using identity_provider.api.services;

namespace identity_provider.api.endpoints;

public static class Admin
{
    public static void AddAdminEndpoints(this WebApplication app)
    {
        app.MapPost(Constants.Endpoints.CREATE_USER, async (HttpContext context, CreateUserRequest request, AdministrationService service) =>
        {
            try
            {
                var result = await service.CreateUser(request.Username, request.Email, request.TemporaryPassword);
                var apiResponse = new ApiResponse<UserCreatedResponse>
                {
                    Success = true,
                    Message = "User created",
                    Data = result,
                };
                return Results.Created(Constants.Endpoints.CREATE_USER, apiResponse);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error creating user");
                throw;
            }
        })
        .WithName("PostCreateUser")
        .WithTags(Constants.Tags.ADMIN)
        .WithOpenApi()
        .RequireAuthorization(Constants.Policies.ADMINISTRATORS_ONLY_POLICY);

        app.MapPost(Constants.Endpoints.RESET_PASSWORD, async (HttpContext context, ResetPasswordRequest request, AdministrationService service) =>
        {
            try
            {
                var result = await service.ResetPassword(request.Username, request.Password);
                var apiResponse = new ApiResponse<ResetPasswordResponse>
                {
                    Success = true,
                    Message = "Password updated successfully.",
                    Data = result,
                };
                return Results.Ok(apiResponse);
            }
            catch (UserNotFoundException ex)
            {
                var apiResponse = new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                };
                return Results.NotFound(apiResponse);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error resetting password");
                throw;
            }
        })
        .WithName("PostResetPassword")
        .WithTags(Constants.Tags.ADMIN)
        .WithOpenApi()
        .RequireAuthorization(Constants.Policies.ADMINISTRATORS_ONLY_POLICY);
    }

}

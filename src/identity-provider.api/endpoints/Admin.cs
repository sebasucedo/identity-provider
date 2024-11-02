using Amazon.CognitoIdentityProvider.Model;
using FluentValidation;
using identity_provider.api.models;
using identity_provider.api.services;

namespace identity_provider.api.endpoints;

public static class Admin
{
    public static void AddAdminEndpoints(this WebApplication app)
    {
        app.MapPost(Constants.Endpoints.CREATE_USER, async (CreateUserRequest request, AdministrationService service) =>
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

        app.MapPost(Constants.Endpoints.RESET_PASSWORD, async (string userId, 
                                                               ResetPasswordModel model,
                                                               IValidator<ResetPasswordModel> validator,
                                                               AdministrationService service) =>
        {
            try
            {
                model.UserId = userId;
                var validationResult = await validator.ValidateAsync(model);
                if (!validationResult.IsValid)
                    return Results.ValidationProblem(validationResult.ToDictionary());

                var result = await service.ResetPassword(model.UserId, model.Password);
                if (!result)
                    return Results.UnprocessableEntity();

                var apiResponse = new ApiResponse<object>
                {
                    Success = true,
                    Message = "Password updated successfully.",
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

        app.MapPost(Constants.Endpoints.DISABLE_USER, async (string userId, AdministrationService service) =>
        {
            try
            {
                var result = await service.DisableUser(userId);
                if (!result)
                    return Results.UnprocessableEntity();

                var apiResponse = new ApiResponse<object>
                {
                    Success = true,
                    Message = "User disabled successfully.",
                };
                return Results.Ok(apiResponse);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, $"Failed to disable user: {userId}");
                throw;
            }
        })
        .WithName("PostDisableUser")
        .WithTags(Constants.Tags.ADMIN)
        .WithOpenApi()
        .RequireAuthorization(Constants.Policies.ADMINISTRATORS_ONLY_POLICY);
    }

}

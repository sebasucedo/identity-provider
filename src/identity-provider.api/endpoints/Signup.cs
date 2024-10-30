using Amazon.CognitoIdentityProvider.Model;
using FluentValidation;
using identity_provider.api.services;

namespace identity_provider.api.endpoints;

public static class Signup
{
    public static void AddSignupEndpoints(this WebApplication app)
    {
        app.MapPost(Constants.Endpoints.SIGNUP, async (IValidator<SignupRequest> validator, 
                                                       SignupRequest signupRequest, 
                                                       AuthenticationService service) =>
        {
            try
            {
                var validationResult = await validator.ValidateAsync(signupRequest);
                if (!validationResult.IsValid)
                    return Results.ValidationProblem(validationResult.ToDictionary());

                var response = await service.SignUp(signupRequest.Username, signupRequest.Email, signupRequest.Password);
                var apiResponse = new ApiResponse<SignUpResponse>
                {
                    Success = true,
                    Message = "User signed up",
                    Data = response,
                };
                return Results.Ok(apiResponse);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error signing up");
                throw;
            }
        })
        .WithName("PostSignUp")
        .WithTags(Constants.Tags.SIGNUP)
        .WithOpenApi();

        app.MapPost(Constants.Endpoints.CONFIRM_SIGNUP, async (IValidator<ConfirmSignupRequest> validator,
                                                               ConfirmSignupRequest confirmSignupRequest,      
                                                               AuthenticationService service) =>
        {

            try
            {
                var validationResult = await validator.ValidateAsync(confirmSignupRequest);
                if (!validationResult.IsValid)
                    return Results.ValidationProblem(validationResult.ToDictionary());

                await service.ConfirmSignUp(confirmSignupRequest.Username, confirmSignupRequest.Code);

                return Results.Ok();
            }
            catch (AliasExistsException ex)
            {
                var response = new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                };
                return Results.Conflict(response);
            }
            catch (ExpiredCodeException ex)
            {
                var response = new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                };
                return Results.BadRequest(response);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error confirming sign up");
                throw;
            }
        })
        .WithName("PostConfirmSignUp")
        .WithTags(Constants.Tags.SIGNUP)
        .WithOpenApi();

        app.MapPost(Constants.Endpoints.RESEND_CONFIRMATION, async (IValidator<ResendConfirmationRequest> validator,
                                                                    ResendConfirmationRequest resendConfirmationRequest,
                                                                    AuthenticationService service) =>
        {
            try
            {
                var validationResult = await validator.ValidateAsync(resendConfirmationRequest);
                if (!validationResult.IsValid)
                    return Results.ValidationProblem(validationResult.ToDictionary());

                await service.ResendConfirmation(resendConfirmationRequest.Username);

                return Results.Ok();
            }
            catch (AliasExistsException ex)
            {
                var response = new ApiResponse<object>
                {
                    Success = false,
                    Message = ex.Message,
                };
                return Results.Conflict(response);
            }
            catch (Exception ex)
            {
                Serilog.Log.Error(ex, "Error resending confirmation");
                throw;
            }
        })
        .WithName("PostResendConfirmation")
        .WithTags(Constants.Tags.SIGNUP)
        .WithOpenApi();
    }

}

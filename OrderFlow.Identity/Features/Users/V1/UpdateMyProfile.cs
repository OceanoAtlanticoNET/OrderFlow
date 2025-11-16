using FluentValidation;
using OrderFlow.Identity.Models.Common;
using OrderFlow.Identity.Models.Users.Requests;
using OrderFlow.Identity.Services.Users;
using System.Security.Claims;

namespace OrderFlow.Identity.Features.Users.V1;

public static class UpdateMyProfile
{
    public static RouteGroupBuilder MapUpdateMyProfile(this RouteGroupBuilder group)
    {
        group.MapPut("/me", HandleAsync)
            .WithName("UpdateMyProfileV1")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Update own profile";
                operation.Description = "Updates the current user's profile information. Requires authentication.";
                return Task.CompletedTask;
            })
            .Produces<Models.Users.Responses.UserResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .DisableAntiforgery();

        return group;
    }

    private static async Task<IResult> HandleAsync(
        ClaimsPrincipal user,
        UpdateProfileRequest? request,
        IUserService userService,
        IValidator<UpdateProfileRequest> validator,
        ILogger<UpdateProfileRequest> logger,
        CancellationToken cancellationToken = default)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return Results.Unauthorized();
        }

        if (request is null)
        {
            return Results.BadRequest(new ErrorResponse
            {
                Errors = ["Request body is required"]
            });
        }

        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            logger.LogWarning("Profile update validation failed for: {UserId}", userId);
            return Results.BadRequest(new ErrorResponse
            {
                Errors = errors,
                Message = "Validation failed"
            });
        }

        logger.LogInformation("Updating profile for user: {UserId}", userId);

        var result = await userService.UpdateCurrentUserProfileAsync(userId, request);

        if (!result.Succeeded)
        {
            logger.LogWarning("Profile update failed for {UserId}: {Errors}",
                userId, string.Join(", ", result.Errors));

            return Results.BadRequest(new ErrorResponse
            {
                Errors = result.Errors,
                Message = "Failed to update profile"
            });
        }

        logger.LogInformation("Profile updated successfully for user: {UserId}", userId);

        return Results.Ok(result.Data);
    }
}

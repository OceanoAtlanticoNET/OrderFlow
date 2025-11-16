using FluentValidation;
using OrderFlow.Identity.Models.Common;
using OrderFlow.Identity.Models.Users.Requests;
using OrderFlow.Identity.Services.Users;
using System.Security.Claims;

namespace OrderFlow.Identity.Features.Users.V1;

public static class ChangeMyPassword
{
    public static RouteGroupBuilder MapChangeMyPassword(this RouteGroupBuilder group)
    {
        group.MapPatch("/me/password", HandleAsync)
            .WithName("ChangeMyPasswordV1")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Change own password";
                operation.Description = "Changes the current user's password. Requires authentication and current password.";
                return Task.CompletedTask;
            })
            .Produces(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .DisableAntiforgery();

        return group;
    }

    private static async Task<IResult> HandleAsync(
        ClaimsPrincipal user,
        ChangePasswordRequest? request,
        IUserService userService,
        IValidator<ChangePasswordRequest> validator,
        ILogger<ChangePasswordRequest> logger,
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
            logger.LogWarning("Password change validation failed for: {UserId}", userId);
            return Results.BadRequest(new ErrorResponse
            {
                Errors = errors,
                Message = "Validation failed"
            });
        }

        logger.LogInformation("Changing password for user: {UserId}", userId);

        var result = await userService.ChangePasswordAsync(
            userId,
            request.CurrentPassword,
            request.NewPassword);

        if (!result.Succeeded)
        {
            logger.LogWarning("Password change failed for {UserId}: {Errors}",
                userId, string.Join(", ", result.Errors));

            // Check if it's a wrong current password error
            if (result.Errors.Any(e => e.Contains("Incorrect password") || e.Contains("current password")))
            {
                return Results.Json(new ErrorResponse
                {
                    Errors = result.Errors,
                    Message = "Current password is incorrect"
                }, statusCode: StatusCodes.Status401Unauthorized);
            }

            return Results.BadRequest(new ErrorResponse
            {
                Errors = result.Errors,
                Message = "Failed to change password"
            });
        }

        logger.LogInformation("Password changed successfully for user: {UserId}", userId);

        return Results.Ok(new { Message = result.Message });
    }
}

using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Identity.Dtos.Users.Requests;
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
            .Accepts<ChangePasswordRequest>("application/json")
            .Produces(StatusCodes.Status200OK)
            .Produces<ValidationProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status401Unauthorized)
            .DisableAntiforgery();

        return group;
    }

    private static async Task<IResult> HandleAsync(
        ClaimsPrincipal user,
        ChangePasswordRequest request,
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

        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.ToDictionary();
            logger.LogWarning("Password change validation failed for: {UserId}", userId);
            return Results.ValidationProblem(errors, title: "Validation failed");
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
                return Results.Problem(
                    title: "Current password is incorrect",
                    detail: string.Join(", ", result.Errors),
                    statusCode: StatusCodes.Status401Unauthorized);
            }

            return Results.Problem(
                title: "Failed to change password",
                detail: string.Join(", ", result.Errors),
                statusCode: StatusCodes.Status400BadRequest);
        }

        logger.LogInformation("Password changed successfully for user: {UserId}", userId);

        return Results.Ok(new { Message = result.Message });
    }
}

using Microsoft.AspNetCore.Mvc;
using OrderFlow.Identity.Services.Users;

namespace OrderFlow.Identity.Features.Users.V1;

public static class UnlockUser
{
    public static RouteGroupBuilder MapUnlockUser(this RouteGroupBuilder group)
    {
        group.MapPost("/{userId}/unlock", HandleAsync)
            .WithName("UnlockUserV1")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Unlock user account";
                operation.Description = "Unlocks a user account to allow login. Requires Admin role.";
                return Task.CompletedTask;
            })
            .Produces(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .DisableAntiforgery();

        return group;
    }

    private static async Task<IResult> HandleAsync(
        string userId,
        IUserService userService,
        ILogger<string> logger)
    {
        logger.LogInformation("Unlocking user: {UserId}", userId);

        var result = await userService.UnlockUserAsync(userId);

        if (!result.Succeeded)
        {
            logger.LogWarning("User unlock failed for {UserId}: {Errors}",
                userId, string.Join(", ", result.Errors));

            if (result.Errors.Any(e => e.Contains("not found")))
            {
                return Results.Problem(
                    title: "User not found",
                    detail: string.Join(", ", result.Errors),
                    statusCode: StatusCodes.Status404NotFound);
            }

            return Results.Problem(
                title: "Failed to unlock user",
                detail: string.Join(", ", result.Errors),
                statusCode: StatusCodes.Status400BadRequest);
        }

        logger.LogInformation("User unlocked successfully: {UserId}", userId);

        return Results.Ok(new { Message = result.Message });
    }
}

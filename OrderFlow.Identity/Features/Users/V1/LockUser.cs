using OrderFlow.Identity.Models.Common;
using OrderFlow.Identity.Services.Users;

namespace OrderFlow.Identity.Features.Users.V1;

public static class LockUser
{
    public record LockUserRequest(DateTimeOffset? LockoutEnd, string? Reason);

    public static RouteGroupBuilder MapLockUser(this RouteGroupBuilder group)
    {
        group.MapPost("/{userId}/lock", HandleAsync)
            .WithName("LockUserV1")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Lock user account";
                operation.Description = "Locks a user account to prevent login. Requires Admin role.";
                return Task.CompletedTask;
            })
            .Produces(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .DisableAntiforgery();

        return group;
    }

    private static async Task<IResult> HandleAsync(
        string userId,
        LockUserRequest? request,
        IUserService userService,
        ILogger<LockUserRequest> logger)
    {
        logger.LogInformation("Locking user: {UserId}", userId);

        var lockoutEnd = request?.LockoutEnd;
        var result = await userService.LockUserAsync(userId, lockoutEnd);

        if (!result.Succeeded)
        {
            logger.LogWarning("User lock failed for {UserId}: {Errors}",
                userId, string.Join(", ", result.Errors));

            if (result.Errors.Any(e => e.Contains("not found")))
            {
                return Results.NotFound(new ErrorResponse
                {
                    Errors = result.Errors,
                    Message = "User not found"
                });
            }

            return Results.BadRequest(new ErrorResponse
            {
                Errors = result.Errors,
                Message = "Failed to lock user"
            });
        }

        logger.LogInformation("User locked successfully: {UserId}", userId);

        return Results.Ok(new { Message = result.Message });
    }
}

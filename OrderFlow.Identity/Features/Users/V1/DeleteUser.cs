using Microsoft.AspNetCore.Mvc;
using OrderFlow.Identity.Services.Users;

namespace OrderFlow.Identity.Features.Users.V1;

public static class DeleteUser
{
    public static RouteGroupBuilder MapDeleteUser(this RouteGroupBuilder group)
    {
        group.MapDelete("/{userId}", HandleAsync)
            .WithName("DeleteUserV1")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Delete user";
                operation.Description = "Permanently deletes a user account. Requires Admin role.";
                return Task.CompletedTask;
            })
            .Produces(StatusCodes.Status204NoContent)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return group;
    }

    private static async Task<IResult> HandleAsync(
        string userId,
        IUserService userService,
        ILogger<string> logger)
    {
        logger.LogInformation("Deleting user: {UserId}", userId);

        var result = await userService.DeleteUserAsync(userId);

        if (!result.Succeeded)
        {
            logger.LogWarning("User deletion failed for {UserId}: {Errors}",
                userId, string.Join(", ", result.Errors));

            if (result.Errors.Any(e => e.Contains("not found")))
            {
                return Results.Problem(
                    title: "User not found",
                    detail: string.Join(", ", result.Errors),
                    statusCode: StatusCodes.Status404NotFound);
            }

            return Results.Problem(
                title: "Failed to delete user",
                detail: string.Join(", ", result.Errors),
                statusCode: StatusCodes.Status400BadRequest);
        }

        logger.LogInformation("User deleted successfully: {UserId}", userId);

        return Results.NoContent();
    }
}

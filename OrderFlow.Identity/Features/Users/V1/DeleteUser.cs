using OrderFlow.Identity.Models.Common;
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
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
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
                return Results.NotFound(new ErrorResponse
                {
                    Errors = result.Errors,
                    Message = "User not found"
                });
            }

            return Results.BadRequest(new ErrorResponse
            {
                Errors = result.Errors,
                Message = "Failed to delete user"
            });
        }

        logger.LogInformation("User deleted successfully: {UserId}", userId);

        return Results.NoContent();
    }
}

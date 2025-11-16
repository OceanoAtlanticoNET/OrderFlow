using OrderFlow.Identity.Models.Common;
using OrderFlow.Identity.Services.Users;

namespace OrderFlow.Identity.Features.Users.V1;

public static class RemoveUserRole
{
    public static RouteGroupBuilder MapRemoveUserRole(this RouteGroupBuilder group)
    {
        group.MapDelete("/{userId}/roles/{roleName}", HandleAsync)
            .WithName("RemoveUserRoleV1")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Remove role from user";
                operation.Description = "Removes a role from a user. Users must have at least one role. Requires Admin role.";
                return Task.CompletedTask;
            })
            .Produces(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return group;
    }

    private static async Task<IResult> HandleAsync(
        string userId,
        string roleName,
        IUserService userService,
        ILogger<string> logger)
    {
        logger.LogInformation("Removing role {Role} from user: {UserId}", roleName, userId);

        var result = await userService.RemoveUserFromRoleAsync(userId, roleName);

        if (!result.Succeeded)
        {
            logger.LogWarning("Role removal failed for {UserId}: {Errors}",
                userId, string.Join(", ", result.Errors));

            if (result.Errors.Any(e => e.Contains("not found")))
            {
                return Results.NotFound(new ErrorResponse
                {
                    Errors = result.Errors,
                    Message = "User or role not found"
                });
            }

            return Results.BadRequest(new ErrorResponse
            {
                Errors = result.Errors,
                Message = "Failed to remove role"
            });
        }

        logger.LogInformation("Role {Role} removed from user {UserId} successfully", roleName, userId);

        return Results.Ok(new { UserId = userId, RoleName = roleName, Message = result.Message });
    }
}

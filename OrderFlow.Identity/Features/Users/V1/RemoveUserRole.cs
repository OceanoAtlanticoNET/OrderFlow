using Microsoft.AspNetCore.Mvc;
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
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
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
                return Results.Problem(
                    title: "User or role not found",
                    detail: string.Join(", ", result.Errors),
                    statusCode: StatusCodes.Status404NotFound);
            }

            return Results.Problem(
                title: "Failed to remove role",
                detail: string.Join(", ", result.Errors),
                statusCode: StatusCodes.Status400BadRequest);
        }

        logger.LogInformation("Role {Role} removed from user {UserId} successfully", roleName, userId);

        return Results.Ok(new { UserId = userId, RoleName = roleName, Message = result.Message });
    }
}

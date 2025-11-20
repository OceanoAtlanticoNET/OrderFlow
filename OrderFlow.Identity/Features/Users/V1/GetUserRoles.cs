using Microsoft.AspNetCore.Mvc;
using OrderFlow.Identity.Services.Users;

namespace OrderFlow.Identity.Features.Users.V1;

public static class GetUserRoles
{
    public static RouteGroupBuilder MapGetUserRoles(this RouteGroupBuilder group)
    {
        group.MapGet("/{userId}/roles", HandleAsync)
            .WithName("GetUserRolesV1")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Get user's roles";
                operation.Description = "Returns all roles assigned to a user. Requires Admin role.";
                return Task.CompletedTask;
            })
            .Produces<IEnumerable<string>>(StatusCodes.Status200OK)
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
        logger.LogInformation("Fetching roles for user: {UserId}", userId);

        var result = await userService.GetUserRolesAsync(userId);

        if (!result.Succeeded)
        {
            logger.LogWarning("Get user roles failed for {UserId}: {Errors}",
                userId, string.Join(", ", result.Errors));

            return Results.Problem(
                title: "User not found",
                detail: string.Join(", ", result.Errors),
                statusCode: StatusCodes.Status404NotFound);
        }

        return Results.Ok(new { UserId = userId, Roles = result.Data });
    }
}

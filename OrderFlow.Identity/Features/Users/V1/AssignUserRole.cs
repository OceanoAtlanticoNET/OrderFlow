using Microsoft.AspNetCore.Mvc;
using OrderFlow.Identity.Services.Users;

namespace OrderFlow.Identity.Features.Users.V1;

public static class AssignUserRole
{
    public static RouteGroupBuilder MapAssignUserRole(this RouteGroupBuilder group)
    {
        group.MapPost("/{userId}/roles/{roleName}", HandleAsync)
            .WithName("AssignUserRoleV1")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Assign role to user";
                operation.Description = "Assigns a role to a user. Requires Admin role.";
                return Task.CompletedTask;
            })
            .Produces(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .DisableAntiforgery();

        return group;
    }

    private static async Task<IResult> HandleAsync(
        string userId,
        string roleName,
        IUserService userService,
        ILogger<string> logger)
    {
        logger.LogInformation("Assigning role {Role} to user: {UserId}", roleName, userId);

        var result = await userService.AddUserToRoleAsync(userId, roleName);

        if (!result.Succeeded)
        {
            logger.LogWarning("Role assignment failed for {UserId}: {Errors}",
                userId, string.Join(", ", result.Errors));

            if (result.Errors.Any(e => e.Contains("not found")))
            {
                return Results.Problem(
                    title: "User or role not found",
                    detail: string.Join(", ", result.Errors),
                    statusCode: StatusCodes.Status404NotFound);
            }

            return Results.Problem(
                title: "Failed to assign role",
                detail: string.Join(", ", result.Errors),
                statusCode: StatusCodes.Status400BadRequest);
        }

        logger.LogInformation("Role {Role} assigned to user {UserId} successfully", roleName, userId);

        return Results.Ok(new { UserId = userId, RoleName = roleName, Message = result.Message });
    }
}

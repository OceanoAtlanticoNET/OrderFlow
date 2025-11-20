using Microsoft.AspNetCore.Mvc;
using OrderFlow.Identity.Services.Roles;

namespace OrderFlow.Identity.Features.Roles.V1;

public static class GetRoleById
{
    public static RouteGroupBuilder MapGetRoleById(this RouteGroupBuilder group)
    {
        group.MapGet("/{roleId}", HandleAsync)
            .WithName("GetRoleByIdV1")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Get role by ID";
                operation.Description = "Returns detailed information about a specific role. Requires Admin role.";
                return Task.CompletedTask;
            })
            .Produces<Dtos.Roles.Responses.RoleDetailResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return group;
    }

    private static async Task<IResult> HandleAsync(
        string roleId,
        IRoleService roleService,
        ILogger<string> logger)
    {
        logger.LogInformation("Fetching role with ID: {RoleId}", roleId);

        var result = await roleService.GetRoleByIdAsync(roleId);

        if (!result.Succeeded)
        {
            logger.LogWarning("Failed to fetch role {RoleId}: {Errors}",
                roleId, string.Join(", ", result.Errors));
            return Results.Problem(
                title: "Role not found",
                detail: string.Join(", ", result.Errors),
                statusCode: StatusCodes.Status404NotFound);
        }

        return Results.Ok(result.Data);
    }
}

using OrderFlow.Identity.Models.Common;
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
            .Produces<Models.Roles.Responses.RoleDetailResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
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
            return Results.NotFound(new ErrorResponse
            {
                Errors = result.Errors,
                Message = "Role not found"
            });
        }

        return Results.Ok(result.Data);
    }
}

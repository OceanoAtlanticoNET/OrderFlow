using OrderFlow.Identity.Models.Common;
using OrderFlow.Identity.Services.Roles;

namespace OrderFlow.Identity.Features.Roles.V1;

public static class GetRoles
{
    public static RouteGroupBuilder MapGetRoles(this RouteGroupBuilder group)
    {
        group.MapGet("/", HandleAsync)
            .WithName("GetRolesV1")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Get all roles";
                operation.Description = "Returns a list of all roles in the system. Requires Admin role.";
                return Task.CompletedTask;
            })
            .Produces<IEnumerable<Models.Roles.Responses.RoleResponse>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return group;
    }

    private static async Task<IResult> HandleAsync(
        IRoleService roleService,
        ILogger<IRoleService> logger)
    {
        logger.LogInformation("Fetching all roles");

        var result = await roleService.GetAllRolesAsync();

        if (!result.Succeeded)
        {
            logger.LogWarning("Failed to fetch roles: {Errors}",
                string.Join(", ", result.Errors));
            return Results.BadRequest(new ErrorResponse
            {
                Errors = result.Errors,
                Message = "Failed to fetch roles"
            });
        }

        return Results.Ok(result.Data);
    }
}

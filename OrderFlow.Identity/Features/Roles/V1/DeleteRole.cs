using OrderFlow.Identity.Models.Common;
using OrderFlow.Identity.Services.Roles;

namespace OrderFlow.Identity.Features.Roles.V1;

public static class DeleteRole
{
    public static RouteGroupBuilder MapDeleteRole(this RouteGroupBuilder group)
    {
        group.MapDelete("/{roleId}", HandleAsync)
            .WithName("DeleteRoleV1")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Delete a role";
                operation.Description = "Deletes a role from the system. Cannot delete roles that have users assigned. Requires Admin role.";
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
        string roleId,
        IRoleService roleService,
        ILogger<string> logger)
    {
        logger.LogInformation("Deleting role: {RoleId}", roleId);

        var result = await roleService.DeleteRoleAsync(roleId);

        if (!result.Succeeded)
        {
            logger.LogWarning("Role deletion failed for {RoleId}: {Errors}",
                roleId, string.Join(", ", result.Errors));

            // Check if it's a not found error
            if (result.Errors.Any(e => e.Contains("not found") || e.Contains("does not exist")))
            {
                return Results.NotFound(new ErrorResponse
                {
                    Errors = result.Errors,
                    Message = "Role not found"
                });
            }

            // Check if it's a constraint error (role has users)
            if (result.Errors.Any(e => e.Contains("users assigned") || e.Contains("cannot be deleted")))
            {
                return Results.BadRequest(new ErrorResponse
                {
                    Errors = result.Errors,
                    Message = "Cannot delete role with assigned users"
                });
            }

            return Results.BadRequest(new ErrorResponse
            {
                Errors = result.Errors,
                Message = "Failed to delete role"
            });
        }

        logger.LogInformation("Role deleted successfully: {RoleId}", roleId);

        return Results.Ok(new { Message = result.Message });
    }
}

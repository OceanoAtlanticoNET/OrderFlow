using Microsoft.AspNetCore.Mvc;
using OrderFlow.Identity.Dtos.Common;
using OrderFlow.Identity.Services.Roles;
using OrderFlow.Shared.Common;

namespace OrderFlow.Identity.Features.Roles.V1;

public static class GetRoleUsers
{
    public static RouteGroupBuilder MapGetRoleUsers(this RouteGroupBuilder group)
    {
        group.MapGet("/{roleId}/users", HandleAsync)
            .WithName("GetRoleUsersV1")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Get users in a role";
                operation.Description = "Returns a paginated list of users assigned to a specific role. Requires Admin role.";
                return Task.CompletedTask;
            })
            .Produces<PaginatedResponse<Dtos.Users.Responses.UserResponse>>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return group;
    }

    private static async Task<IResult> HandleAsync(
        string roleId,
        [AsParameters] PaginationQuery pagination,
        IRoleService roleService,
        ILogger<PaginationQuery> logger)
    {
        logger.LogInformation("Fetching users for role: {RoleId}, Page: {Page}, PageSize: {PageSize}",
            roleId, pagination.Page, pagination.PageSize);

        var result = await roleService.GetUsersInRoleAsync(roleId, pagination);

        // Check if role was not found (empty result with no total count)
        if (result.Pagination.TotalCount == 0 && !result.Data.Any())
        {
            logger.LogWarning("Role not found: {RoleId}", roleId);
        }

        return Results.Ok(new PaginatedResponse<Dtos.Users.Responses.UserResponse>
        {
            Data = result.Data,
            Pagination = result.Pagination
        });
    }
}

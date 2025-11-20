using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Identity.Dtos.Roles.Requests;
using OrderFlow.Identity.Services.Roles;

namespace OrderFlow.Identity.Features.Roles.V1;

public static class UpdateRole
{
    public static RouteGroupBuilder MapUpdateRole(this RouteGroupBuilder group)
    {
        group.MapPut("/{roleId}", HandleAsync)
            .WithName("UpdateRoleV1")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Update a role";
                operation.Description = "Updates an existing role's name. Requires Admin role.";
                return Task.CompletedTask;
            })
            .Accepts<UpdateRoleRequest>("application/json")
            .Produces<Dtos.Roles.Responses.RoleResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .DisableAntiforgery();

        return group;
    }

    private static async Task<IResult> HandleAsync(
        string roleId,
        UpdateRoleRequest request,
        IRoleService roleService,
        IValidator<UpdateRoleRequest> validator,
        ILogger<UpdateRoleRequest> logger,
        CancellationToken cancellationToken = default)
    {

        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.ToDictionary();
            logger.LogWarning("Role update validation failed for: {RoleId}", roleId);
            return Results.ValidationProblem(errors, title: "Validation failed");
        }

        logger.LogInformation("Updating role {RoleId} to name: {NewName}", roleId, request.RoleName);

        var result = await roleService.UpdateRoleAsync(roleId, request.RoleName);

        if (!result.Succeeded)
        {
            logger.LogWarning("Role update failed for {RoleId}: {Errors}",
                roleId, string.Join(", ", result.Errors));

            // Check if it's a not found error
            if (result.Errors.Any(e => e.Contains("not found") || e.Contains("does not exist")))
            {
                return Results.Problem(
                    title: "Role not found",
                    detail: string.Join(", ", result.Errors),
                    statusCode: StatusCodes.Status404NotFound);
            }

            return Results.Problem(
                title: "Failed to update role",
                detail: string.Join(", ", result.Errors),
                statusCode: StatusCodes.Status400BadRequest);
        }

        logger.LogInformation("Role updated successfully: {RoleId}", roleId);

        return Results.Ok(result.Data);
    }
}

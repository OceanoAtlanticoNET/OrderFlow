using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using OrderFlow.Identity.Dtos.Users.Requests;
using OrderFlow.Identity.Services.Users;

namespace OrderFlow.Identity.Features.Users.V1;

public static class UpdateUser
{
    public static RouteGroupBuilder MapUpdateUser(this RouteGroupBuilder group)
    {
        group.MapPut("/{userId}", HandleAsync)
            .WithName("UpdateUserV1")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Update user information";
                operation.Description = "Updates a user's information. Requires Admin role.";
                return Task.CompletedTask;
            })
            .Accepts<UpdateUserRequest>("application/json")
            .Produces<Dtos.Users.Responses.UserResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status400BadRequest)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .DisableAntiforgery();

        return group;
    }

    private static async Task<IResult> HandleAsync(
        string userId,
        UpdateUserRequest request,
        IUserService userService,
        IValidator<UpdateUserRequest> validator,
        ILogger<UpdateUserRequest> logger,
        CancellationToken cancellationToken = default)
    {

        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.ToDictionary();
            logger.LogWarning("User update validation failed for: {UserId}", userId);
            return Results.ValidationProblem(errors, title: "Validation failed");
        }

        var result = await userService.UpdateUserAsync(userId, request);

        if (!result.Succeeded)
        {
            logger.LogWarning("User update failed for {UserId}: {Errors}",
                userId, string.Join(", ", result.Errors));

            if (result.Errors.Any(e => e.Contains("not found")))
            {
                return Results.Problem(
                    title: "User not found",
                    detail: string.Join(", ", result.Errors),
                    statusCode: StatusCodes.Status404NotFound);
            }

            return Results.Problem(
                title: "Failed to update user",
                detail: string.Join(", ", result.Errors),
                statusCode: StatusCodes.Status400BadRequest);
        }

        logger.LogInformation("User updated successfully: {UserId}", userId);

        return Results.Ok(result.Data);
    }
}

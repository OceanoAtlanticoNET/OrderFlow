using FluentValidation;
using OrderFlow.Identity.Models.Common;
using OrderFlow.Identity.Models.Users.Requests;
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
            .Produces<Models.Users.Responses.UserResponse>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .DisableAntiforgery();

        return group;
    }

    private static async Task<IResult> HandleAsync(
        string userId,
        UpdateUserRequest? request,
        IUserService userService,
        IValidator<UpdateUserRequest> validator,
        ILogger<UpdateUserRequest> logger,
        CancellationToken cancellationToken = default)
    {
        if (request is null)
        {
            return Results.BadRequest(new ErrorResponse
            {
                Errors = ["Request body is required"]
            });
        }

        var validationResult = await validator.ValidateAsync(request, cancellationToken);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors.Select(e => e.ErrorMessage);
            logger.LogWarning("User update validation failed for: {UserId}", userId);
            return Results.BadRequest(new ErrorResponse
            {
                Errors = errors,
                Message = "Validation failed"
            });
        }

        var result = await userService.UpdateUserAsync(userId, request);

        if (!result.Succeeded)
        {
            logger.LogWarning("User update failed for {UserId}: {Errors}",
                userId, string.Join(", ", result.Errors));

            if (result.Errors.Any(e => e.Contains("not found")))
            {
                return Results.NotFound(new ErrorResponse
                {
                    Errors = result.Errors,
                    Message = "User not found"
                });
            }

            return Results.BadRequest(new ErrorResponse
            {
                Errors = result.Errors,
                Message = "Failed to update user"
            });
        }

        logger.LogInformation("User updated successfully: {UserId}", userId);

        return Results.Ok(result.Data);
    }
}

using FluentValidation;
using OrderFlow.Identity.Models.Common;
using OrderFlow.Identity.Models.Users.Requests;
using OrderFlow.Identity.Services.Users;

namespace OrderFlow.Identity.Features.Users.V1;

public static class CreateUser
{
    public static RouteGroupBuilder MapCreateUser(this RouteGroupBuilder group)
    {
        group.MapPost("/", HandleAsync)
            .WithName("CreateUserV1")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Create a new user";
                operation.Description = "Creates a new user account with specified roles. Requires Admin role.";
                return Task.CompletedTask;
            })
            .Produces<Models.Users.Responses.UserResponse>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden)
            .DisableAntiforgery();

        return group;
    }

    private static async Task<IResult> HandleAsync(
        CreateUserRequest? request,
        IUserService userService,
        IValidator<CreateUserRequest> validator,
        ILogger<CreateUserRequest> logger,
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
            logger.LogWarning("User creation validation failed");
            return Results.BadRequest(new ErrorResponse
            {
                Errors = errors,
                Message = "Validation failed"
            });
        }

        var result = await userService.CreateUserAsync(request);

        if (!result.Succeeded)
        {
            logger.LogWarning("User creation failed: {Errors}", string.Join(", ", result.Errors));
            return Results.BadRequest(new ErrorResponse
            {
                Errors = result.Errors,
                Message = "Failed to create user"
            });
        }

        logger.LogInformation("User created successfully: {UserId}", result.Data!.UserId);

        return Results.Created($"/api/v1/admin/users/{result.Data.UserId}", result.Data);
    }
}

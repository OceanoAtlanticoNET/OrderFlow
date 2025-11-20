using Microsoft.AspNetCore.Mvc;
using OrderFlow.Identity.Services.Users;

namespace OrderFlow.Identity.Features.Users.V1;

public static class GetUserById
{
    public static RouteGroupBuilder MapGetUserById(this RouteGroupBuilder group)
    {
        group.MapGet("/{userId}", HandleAsync)
            .WithName("GetUserByIdV1")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Get user by ID";
                operation.Description = "Returns detailed information about a specific user. Requires Admin role.";
                return Task.CompletedTask;
            })
            .Produces<Dtos.Users.Responses.UserDetailResponse>(StatusCodes.Status200OK)
            .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return group;
    }

    private static async Task<IResult> HandleAsync(
        string userId,
        IUserService userService,
        ILogger<string> logger)
    {
        logger.LogInformation("Fetching user details for: {UserId}", userId);

        var result = await userService.GetUserByIdAsync(userId);

        if (!result.Succeeded)
        {
            return Results.Problem(
                title: "User not found",
                detail: string.Join(", ", result.Errors),
                statusCode: StatusCodes.Status404NotFound);
        }

        return Results.Ok(result.Data);
    }
}

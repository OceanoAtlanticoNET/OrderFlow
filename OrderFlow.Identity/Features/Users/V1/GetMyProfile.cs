using OrderFlow.Identity.Models.Common;
using OrderFlow.Identity.Services.Users;
using System.Security.Claims;

namespace OrderFlow.Identity.Features.Users.V1;

public static class GetMyProfile
{
    public static RouteGroupBuilder MapGetMyProfile(this RouteGroupBuilder group)
    {
        group.MapGet("/me", HandleAsync)
            .WithName("GetMyProfileV1")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Get own profile";
                operation.Description = "Returns the current user's profile information. Requires authentication.";
                return Task.CompletedTask;
            })
            .Produces<Models.Users.Responses.UserDetailResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        return group;
    }

    private static async Task<IResult> HandleAsync(
        ClaimsPrincipal user,
        IUserService userService,
        ILogger<ClaimsPrincipal> logger)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return Results.Unauthorized();
        }

        logger.LogInformation("Fetching profile for user: {UserId}", userId);

        var result = await userService.GetCurrentUserProfileAsync(userId);

        if (!result.Succeeded)
        {
            logger.LogWarning("Failed to fetch profile for {UserId}: {Errors}",
                userId, string.Join(", ", result.Errors));
            return Results.Unauthorized();
        }

        return Results.Ok(result.Data);
    }
}

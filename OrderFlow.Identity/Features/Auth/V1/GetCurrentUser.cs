using System.Security.Claims;
using OrderFlow.Identity.Services.Auth;

namespace OrderFlow.Identity.Features.Auth.V1;

public static class GetCurrentUser
{
    public sealed record CurrentUserResponse(
        string UserId,
        string Email,
        IEnumerable<string> Roles
    );

    public static IEndpointRouteBuilder MapGetCurrentUser(this IEndpointRouteBuilder endpoints)
    {
        var authGroup = endpoints.MapAuthGroup();

        authGroup.MapGet("/me", HandleAsync)
            .RequireAuthorization()
            .WithName("GetCurrentUserV1")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Get current authenticated user";
                operation.Description = "Returns the current user's information from JWT token claims. Requires authentication.";
                return Task.CompletedTask;
            })
            .Produces<CurrentUserResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        return endpoints;
    }

    private static async Task<IResult> HandleAsync(
        ClaimsPrincipal user,
        IAuthService authService)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);

        if (userId is null)
        {
            return Results.Unauthorized();
        }

        // Call authentication service
        var result = await authService.GetCurrentUserAsync(userId);

        if (!result.Succeeded)
        {
            return Results.Unauthorized();
        }

        return Results.Ok(result.Data);
    }
}
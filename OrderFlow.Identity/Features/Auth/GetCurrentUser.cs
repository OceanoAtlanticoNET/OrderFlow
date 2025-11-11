using System.Security.Claims;

namespace OrderFlow.Identity.Features.Auth;

public static class GetCurrentUser
{
    public sealed record Response(
        string UserId,
        string Email,
        IEnumerable<string> Roles
    );

    public static IEndpointRouteBuilder MapGetCurrentUser(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/auth/me", HandleAsync)
            .RequireAuthorization()
            .WithName("GetCurrentUser")
            .WithTags("Authentication")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get current authenticated user";
                operation.Description = "Returns the current user's information from JWT token claims. Requires authentication.";
                return operation;
            })
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

        return endpoints;
    }

    private static IResult HandleAsync(ClaimsPrincipal user)
    {
        var userId = user.FindFirstValue(ClaimTypes.NameIdentifier);
        var email = user.FindFirstValue(ClaimTypes.Email);
        var roles = user.FindAll(ClaimTypes.Role).Select(c => c.Value);

        if (userId is null || email is null)
        {
            return Results.Unauthorized();
        }

        var response = new Response(
            UserId: userId,
            Email: email,
            Roles: roles
        );

        return Results.Ok(response);
    }
}

using System.Security.Claims;

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
            .WithOpenApi(operation =>
            {
                operation.Summary = "Get current authenticated user";
                operation.Description = "Returns the current user's information from JWT token claims. Requires authentication.";
                return operation;
            })
            .Produces<CurrentUserResponse>(StatusCodes.Status200OK)
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

        var response = new CurrentUserResponse(
            UserId: userId,
            Email: email,
            Roles: roles
        );

        return Results.Ok(response);
    }
}
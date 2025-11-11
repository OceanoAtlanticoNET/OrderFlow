using OrderFlow.Identity.Data;

namespace OrderFlow.Identity.Features.Auth;

public static class AdminOnly
{
    public sealed record Response(
        string Message,
        DateTime Timestamp
    );

    public static IEndpointRouteBuilder MapAdminOnly(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/auth/admin-only", HandleAsync)
            .RequireAuthorization(policy => policy.RequireRole(Roles.Admin))
            .WithName("AdminOnly")
            .WithTags("Authentication")
            .WithOpenApi(operation =>
            {
                operation.Summary = "Admin only endpoint";
                operation.Description = "Test endpoint that requires Admin role. Returns 403 if user is not an admin.";
                return operation;
            })
            .Produces<Response>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return endpoints;
    }

    private static IResult HandleAsync()
    {
        var response = new Response(
            Message: "You are an admin!",
            Timestamp: DateTime.UtcNow
        );

        return Results.Ok(response);
    }
}

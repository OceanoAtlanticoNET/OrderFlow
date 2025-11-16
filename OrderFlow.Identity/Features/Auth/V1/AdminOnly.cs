using OrderFlow.Identity.Data;

namespace OrderFlow.Identity.Features.Auth.V1;

public static class AdminOnly
{
    public sealed record AdminOnlyResponse(
        string Message,
        DateTime Timestamp
    );

    public static IEndpointRouteBuilder MapAdminOnly(this IEndpointRouteBuilder endpoints)
    {
        var authGroup = endpoints.MapAuthGroup();

        authGroup.MapGet("/admin-only", HandleAsync)
            .RequireAuthorization(policy => policy.RequireRole(Data.Roles.Admin))
            .WithName("AdminOnlyV1")
            .AddOpenApiOperationTransformer((operation, context, ct) =>
            {
                operation.Summary = "Admin only endpoint";
                operation.Description = "Test endpoint that requires Admin role. Returns 403 if user is not an admin.";
                return Task.CompletedTask;
            })
            .Produces<AdminOnlyResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status403Forbidden);

        return endpoints;
    }

    private static IResult HandleAsync()
    {
        var response = new AdminOnlyResponse(
            Message: "You are an admin!",
            Timestamp: DateTime.UtcNow
        );

        return Results.Ok(response);
    }
}
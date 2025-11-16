using Asp.Versioning.Builder;
using Asp.Versioning;

namespace OrderFlow.Identity.Features.Auth.V1;

/// <summary>
/// Provides shared configuration for V1 Authentication API group.
/// Centralizes version setup and eliminates code duplication across V1 endpoints.
/// </summary>
public static class AuthGroup
{
    /// <summary>
    /// Creates and configures the V1 authentication API group with common settings.
    /// </summary>
    /// <param name="endpoints">The endpoint route builder</param>
    /// <returns>Configured route group builder for V1 auth endpoints</returns>
    public static RouteGroupBuilder MapAuthGroup(this IEndpointRouteBuilder endpoints)
    {
        var versionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        return endpoints
            .MapGroup("/api/v{version:apiVersion}/auth")
            .WithApiVersionSet(versionSet)
            .WithTags("Authentication V1");
    }
}
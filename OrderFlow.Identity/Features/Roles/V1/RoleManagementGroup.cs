using Asp.Versioning;
using Asp.Versioning.Builder;

namespace OrderFlow.Identity.Features.Roles.V1;

/// <summary>
/// Provides shared configuration for V1 Role Management API group.
/// Centralizes version setup and eliminates code duplication across V1 role endpoints.
/// </summary>
public static class RoleManagementGroup
{
    /// <summary>
    /// Creates and configures the V1 role management API group with common settings.
    /// </summary>
    public static RouteGroupBuilder MapRoleManagementGroup(this IEndpointRouteBuilder endpoints)
    {
        var versionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        return endpoints
            .MapGroup("/api/v{version:apiVersion}/admin/roles")
            .WithApiVersionSet(versionSet)
            .WithTags("Role Management")
            .RequireAuthorization(policy => policy.RequireRole(Data.Roles.Admin));
    }
}

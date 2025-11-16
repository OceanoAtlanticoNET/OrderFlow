using Asp.Versioning;
using Asp.Versioning.Builder;

namespace OrderFlow.Identity.Features.Users.V1;

/// <summary>
/// Provides shared configuration for V1 User Management API groups.
/// Centralizes version setup and eliminates code duplication across V1 user endpoints.
/// </summary>
public static class UserManagementGroup
{
    /// <summary>
    /// Creates and configures the V1 admin user management API group with common settings.
    /// </summary>
    public static RouteGroupBuilder MapAdminUserGroup(this IEndpointRouteBuilder endpoints)
    {
        var versionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        return endpoints
            .MapGroup("/api/v{version:apiVersion}/admin/users")
            .WithApiVersionSet(versionSet)
            .WithTags("User Management")
            .RequireAuthorization(policy => policy.RequireRole(Data.Roles.Admin));
    }

    /// <summary>
    /// Creates and configures the V1 user self-management API group with common settings.
    /// </summary>
    public static RouteGroupBuilder MapUserSelfGroup(this IEndpointRouteBuilder endpoints)
    {
        var versionSet = endpoints.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        return endpoints
            .MapGroup("/api/v{version:apiVersion}/users")
            .WithApiVersionSet(versionSet)
            .WithTags("User Self-Management")
            .RequireAuthorization();
    }
}

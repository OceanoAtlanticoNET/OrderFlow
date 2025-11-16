namespace OrderFlow.Identity.Features.Roles.V1;

public static class RoleGroup
{
    public static IEndpointRouteBuilder MapRoleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Role Management (grouped under "Role Management" tag)
        var roleGroup = endpoints.MapRoleManagementGroup();
        roleGroup.MapGetRoles();
        roleGroup.MapGetRoleById();
        roleGroup.MapCreateRole();
        roleGroup.MapUpdateRole();
        roleGroup.MapDeleteRole();
        roleGroup.MapGetRoleUsers();

        return endpoints;
    }
}

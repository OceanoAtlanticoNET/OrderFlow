namespace OrderFlow.Identity.Features.Users.V1;

public static class UserGroup
{
    public static IEndpointRouteBuilder MapUserEndpoints(this IEndpointRouteBuilder endpoints)
    {
        // Admin - User Management (grouped under "User Management" tag)
        var adminGroup = endpoints.MapAdminUserGroup();
        adminGroup.MapGetUsers();
        adminGroup.MapGetUserById();
        adminGroup.MapCreateUser();
        adminGroup.MapUpdateUser();
        adminGroup.MapDeleteUser();
        adminGroup.MapLockUser();
        adminGroup.MapUnlockUser();
        adminGroup.MapGetUserRoles();
        adminGroup.MapAssignUserRole();
        adminGroup.MapRemoveUserRole();

        // Self-Management (grouped under "User Self-Management" tag)
        var selfGroup = endpoints.MapUserSelfGroup();
        selfGroup.MapGetMyProfile();
        selfGroup.MapUpdateMyProfile();
        selfGroup.MapChangeMyPassword();

        return endpoints;
    }
}

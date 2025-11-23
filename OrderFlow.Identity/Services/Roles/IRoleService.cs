using OrderFlow.Identity.Dtos.Roles.Responses;
using OrderFlow.Identity.Dtos.Users.Responses;
using OrderFlow.Shared.Common;

namespace OrderFlow.Identity.Services.Roles;

/// <summary>
/// Service for role management operations
/// </summary>
public interface IRoleService
{
    /// <summary>
    /// Get all roles
    /// </summary>
    Task<ServiceResult<IEnumerable<RoleResponse>>> GetAllRolesAsync();

    /// <summary>
    /// Get role details by ID
    /// </summary>
    Task<ServiceResult<RoleDetailResponse>> GetRoleByIdAsync(string roleId);

    /// <summary>
    /// Create a new role
    /// </summary>
    Task<ServiceResult<RoleResponse>> CreateRoleAsync(string roleName);

    /// <summary>
    /// Update role name
    /// </summary>
    Task<ServiceResult<RoleResponse>> UpdateRoleAsync(string roleId, string newRoleName);

    /// <summary>
    /// Delete role
    /// </summary>
    Task<ServiceResult> DeleteRoleAsync(string roleId);

    /// <summary>
    /// Get users in a specific role (paginated)
    /// </summary>
    Task<PaginatedResult<UserResponse>> GetUsersInRoleAsync(string roleId, PaginationQuery pagination);
}

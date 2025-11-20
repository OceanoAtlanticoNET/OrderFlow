namespace OrderFlow.Identity.Dtos.Roles.Requests;

/// <summary>
/// Request model for updating a role
/// </summary>
public record UpdateRoleRequest
{
    /// <summary>
    /// New role name
    /// </summary>
    public required string RoleName { get; init; }
}

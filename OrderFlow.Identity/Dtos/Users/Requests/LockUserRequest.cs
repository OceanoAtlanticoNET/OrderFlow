namespace OrderFlow.Identity.Dtos.Users.Requests;

/// <summary>
/// Request for locking a user account
/// </summary>
public record LockUserRequest
{
    /// <summary>
    /// When the lockout should end (null means indefinite)
    /// </summary>
    public DateTimeOffset? LockoutEnd { get; init; }
}

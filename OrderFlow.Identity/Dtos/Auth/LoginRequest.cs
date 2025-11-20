namespace OrderFlow.Identity.Dtos.Auth;

/// <summary>
/// Request model for user login
/// </summary>
public record LoginRequest
{
    /// <summary>
    /// User email address
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// User password
    /// </summary>
    public required string Password { get; init; }
}

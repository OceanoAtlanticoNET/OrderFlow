namespace OrderFlow.Identity.Dtos.Auth;

/// <summary>
/// Request model for user registration
/// </summary>
public record RegisterRequest
{
    /// <summary>
    /// User email address
    /// </summary>
    public required string Email { get; init; }

    /// <summary>
    /// User password
    /// </summary>
    public required string Password { get; init; }

    /// <summary>
    /// Password confirmation
    /// </summary>
    public required string ConfirmPassword { get; init; }
}

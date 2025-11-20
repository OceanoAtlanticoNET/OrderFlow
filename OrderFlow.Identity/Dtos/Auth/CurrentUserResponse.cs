namespace OrderFlow.Identity.Dtos.Auth;

/// <summary>
/// Response model for current user information
/// </summary>
public record CurrentUserResponse
{
    public required string UserId { get; init; }
    public required string Email { get; init; }
    public required IEnumerable<string> Roles { get; init; }
}

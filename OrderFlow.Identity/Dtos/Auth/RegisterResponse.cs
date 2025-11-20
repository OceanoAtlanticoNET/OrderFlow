namespace OrderFlow.Identity.Dtos.Auth;

/// <summary>
/// Response model for user registration
/// </summary>
public record RegisterResponse
{
    public required string UserId { get; init; }
    public required string Email { get; init; }
    public required string Message { get; init; }
}

namespace OrderFlow.Identity.Dtos.Auth;

/// <summary>
/// Response model for successful login
/// </summary>
public record LoginResponse
{
    public required string AccessToken { get; init; }
    public required string TokenType { get; init; }
    public required int ExpiresIn { get; init; }
    public required string UserId { get; init; }
    public required string Email { get; init; }
    public required IEnumerable<string> Roles { get; init; }
}

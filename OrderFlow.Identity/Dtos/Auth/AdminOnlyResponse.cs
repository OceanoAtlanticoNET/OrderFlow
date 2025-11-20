namespace OrderFlow.Identity.Dtos.Auth;

/// <summary>
/// Response model for admin-only endpoint
/// </summary>
public record AdminOnlyResponse
{
    public required string Message { get; init; }
    public required DateTime Timestamp { get; init; }
}

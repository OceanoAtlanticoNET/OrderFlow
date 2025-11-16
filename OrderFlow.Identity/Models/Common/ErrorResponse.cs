namespace OrderFlow.Identity.Models.Common;

/// <summary>
/// Standard error response model for API errors
/// </summary>
public record ErrorResponse
{
    /// <summary>
    /// List of error messages
    /// </summary>
    public required IEnumerable<string> Errors { get; init; }

    /// <summary>
    /// General error message (optional)
    /// </summary>
    public string? Message { get; init; }

    /// <summary>
    /// HTTP status code (optional)
    /// </summary>
    public int? StatusCode { get; init; }
}

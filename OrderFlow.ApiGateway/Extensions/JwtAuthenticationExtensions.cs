using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using OrderFlow.Shared.Extensions;

namespace OrderFlow.ApiGateway.Extensions;

/// <summary>
/// Gateway-specific JWT authentication configuration with logging events.
/// </summary>
public static class GatewayJwtAuthenticationExtensions
{
    /// <summary>
    /// Adds JWT authentication with gateway-specific logging events.
    /// </summary>
    public static IServiceCollection AddGatewayJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        return services.AddJwtAuthentication(configuration, events =>
        {
            events.OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILogger<Program>>();

                logger.LogWarning("Auth failed: {Exception}", context.Exception.Message);

                if (context.Exception is SecurityTokenExpiredException)
                {
                    context.Response.Headers.Append("Token-Expired", "true");
                }

                return Task.CompletedTask;
            };

            events.OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILogger<Program>>();

                var userName = context.Principal?.Identity?.Name ?? "Unknown";
                var userId = context.Principal?.FindFirst("sub")?.Value
                    ?? context.Principal?.FindFirst("nameid")?.Value
                    ?? "Unknown";

                logger.LogInformation("Token validated: {UserName} ({UserId})", userName, userId);

                return Task.CompletedTask;
            };

            events.OnChallenge = context =>
            {
                var logger = context.HttpContext.RequestServices
                    .GetRequiredService<ILogger<Program>>();

                logger.LogWarning("Auth challenge: {Error} - {ErrorDescription}",
                    context.Error, context.ErrorDescription);

                return Task.CompletedTask;
            };
        });
    }
}

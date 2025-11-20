using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace OrderFlow.Identity.Extensions;

public static class JwtAuthenticationExtensions
{
    /// <summary>
    /// Adds JWT Bearer authentication with standard configuration
    /// </summary>
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Use fallback values for build-time/design-time scenarios (e.g., OpenAPI generation)
        var jwtSecret = configuration["Jwt:Secret"]
            ?? "build-time-secret-key-minimum-32-characters-required-for-hmac-sha256";
        var jwtIssuer = configuration["Jwt:Issuer"]
            ?? "build-time-issuer";
        var jwtAudience = configuration["Jwt:Audience"]
            ?? "build-time-audience";

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtIssuer,
                ValidAudience = jwtAudience,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ClockSkew = TimeSpan.Zero // Remove default 5 minute tolerance
            };

            // Add event handlers for better debugging
            options.Events = new JwtBearerEvents
            {
                OnAuthenticationFailed = context =>
                {
                    if (context.Exception is SecurityTokenExpiredException)
                    {
                        context.Response.Headers.Append("Token-Expired", "true");
                    }
                    return Task.CompletedTask;
                },
                OnChallenge = context =>
                {
                    // Log authorization failures for debugging
                    context.HttpContext.RequestServices
                        .GetRequiredService<ILogger<Program>>()
                        .LogWarning("Authentication challenge: {Error} - {ErrorDescription}",
                            context.Error, context.ErrorDescription);
                    return Task.CompletedTask;
                }
            };
        });

        return services;
    }
}

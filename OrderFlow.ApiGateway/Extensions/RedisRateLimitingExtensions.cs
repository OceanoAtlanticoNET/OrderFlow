using RedisRateLimiting;
using RedisRateLimiting.AspNetCore;
using StackExchange.Redis;
using System.Threading.RateLimiting;

namespace OrderFlow.ApiGateway.Extensions;

public static class RedisRateLimitingExtensions
{
    public static IServiceCollection AddRedisRateLimiting(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddRateLimiter(options =>
        {
            // Anonymous policy for public endpoints (100 req/min)
            options.AddPolicy("anonymous", context =>
            {
                var redis = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();
                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                return RedisRateLimitPartition.GetFixedWindowRateLimiter(
                    $"ip:{ipAddress}",
                    _ => new RedisFixedWindowRateLimiterOptions
                    {
                        ConnectionMultiplexerFactory = () => redis,
                        PermitLimit = 100,
                        Window = TimeSpan.FromMinutes(1)
                    });
            });

            // Authenticated users policy (250 req/min)
            options.AddPolicy("authenticated", context =>
            {
                var redis = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();

                var userId = context.User.FindFirst("sub")?.Value
                    ?? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                    ?? "unknown";

                return RedisRateLimitPartition.GetFixedWindowRateLimiter(
                    $"user:{userId}",
                    _ => new RedisFixedWindowRateLimiterOptions
                    {
                        ConnectionMultiplexerFactory = () => redis,
                        PermitLimit = 250,
                        Window = TimeSpan.FromMinutes(1)
                    });
            });

            // Handle rate limit rejections
            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue)
                    ? ((TimeSpan)retryAfterValue).TotalSeconds
                    : 60;

                context.HttpContext.Response.Headers.RetryAfter = retryAfter.ToString();

                await context.HttpContext.Response.WriteAsJsonAsync(new
                {
                    error = "Too many requests",
                    message = "Rate limit exceeded. Please try again later.",
                    retryAfter = $"{retryAfter} seconds"
                }, cancellationToken);
            };
        });

        return services;
    }
}

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
        var settings = configuration.GetSection("RateLimiting").Get<RateLimitSettings>()
            ?? new RateLimitSettings();

        services.AddRateLimiter(options =>
        {
            // Customer policy - partitioned by user ID or IP
            options.AddPolicy("customer", context =>
            {
                var redis = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();
                var loggerFactory = context.RequestServices.GetRequiredService<ILoggerFactory>();
                var logger = loggerFactory.CreateLogger("RateLimiting");

                // Get partition key: user ID for authenticated, IP for anonymous
                var isAuthenticated = context.User.Identity?.IsAuthenticated == true;

                // Get user ID from sub claim (preferred) or NameIdentifier
                var userId = context.User.FindFirst("sub")?.Value
                    ?? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                // Get IP address (keep IPv6 for better client isolation)
                var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

                var partitionKey = isAuthenticated && userId != null
                    ? $"user:{userId}"
                    : $"ip:{ipAddress}";

                logger.LogInformation("Rate limit partition: {PartitionKey} (Auth: {IsAuth}, UserId: {UserId}, IP: {IP})",
                    partitionKey, isAuthenticated, userId ?? "none", ipAddress);

                return RedisRateLimitPartition.GetFixedWindowRateLimiter(
                    partitionKey,
                    _ => new RedisFixedWindowRateLimiterOptions
                    {
                        ConnectionMultiplexerFactory = () => redis,
                        PermitLimit = settings.Customer.PermitLimit,
                        Window = settings.Customer.Window
                    });
            });

            // Admin policy - partitioned by user ID
            options.AddPolicy("admin", context =>
            {
                var redis = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();

                // Get user ID from sub claim (preferred) or NameIdentifier
                var userId = context.User.FindFirst("sub")?.Value
                    ?? context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                    ?? "unknown";

                var partitionKey = $"admin:{userId}";

                return RedisRateLimitPartition.GetFixedWindowRateLimiter(
                    partitionKey,
                    _ => new RedisFixedWindowRateLimiterOptions
                    {
                        ConnectionMultiplexerFactory = () => redis,
                        PermitLimit = settings.Admin.PermitLimit,
                        Window = settings.Admin.Window
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

public class RateLimitSettings
{
    public RateLimitPolicy Customer { get; set; } = new() { PermitLimit = 100, Window = TimeSpan.FromMinutes(1) };
    public RateLimitPolicy Admin { get; set; } = new() { PermitLimit = 2000, Window = TimeSpan.FromMinutes(1) };
}

public class RateLimitPolicy
{
    public int PermitLimit { get; set; }
    public TimeSpan Window { get; set; }
}

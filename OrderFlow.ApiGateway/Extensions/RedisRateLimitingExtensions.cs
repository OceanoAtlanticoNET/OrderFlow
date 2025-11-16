using StackExchange.Redis;
using System.Threading.RateLimiting;

namespace OrderFlow.ApiGateway.Extensions;

/// <summary>
/// Redis-backed distributed rate limiting for multi-instance API Gateway deployments
/// Use this when you have multiple gateway instances behind a load balancer
/// </summary>
public static class RedisRateLimitingExtensions
{
    /// <summary>
    /// Adds distributed rate limiting using Redis as the backing store
    /// This ensures rate limits are enforced across all gateway instances
    /// </summary>
    public static IServiceCollection AddRedisRateLimiting(
        this IServiceCollection services)
    {
        services.AddRateLimiter(options =>
        {
            options.GlobalLimiter = PartitionedRateLimiter.CreateChained(
                // First partition by user/IP
                PartitionedRateLimiter.Create<HttpContext, string>(context =>
                {
                    var userId = context.User.Identity?.Name ?? "anonymous";
                    var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                    var partitionKey = context.User.Identity?.IsAuthenticated == true
                        ? $"user:{userId}"
                        : $"ip:{ipAddress}";

                    // Get Redis connection from DI
                    var redis = context.RequestServices.GetRequiredService<IConnectionMultiplexer>();
                    var db = redis.GetDatabase();

                    return RateLimitPartition.Get(
                        partitionKey: partitionKey,
                        factory: key => new RedisRateLimiter(
                            database: db,
                            partitionKey: key,
                            permitLimit: context.User.Identity?.IsAuthenticated == true ? 100 : 20,
                            window: TimeSpan.FromMinutes(1)
                        ));
                }));

            options.OnRejected = async (context, cancellationToken) =>
            {
                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;

                var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfterValue)
                    ? retryAfterValue.TotalSeconds
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

/// <summary>
/// Custom Redis-backed rate limiter
/// Stores rate limit counters in Redis for distributed enforcement
/// </summary>
public class RedisRateLimiter : RateLimiter
{
    private readonly IDatabase _database;
    private readonly string _partitionKey;
    private readonly int _permitLimit;
    private readonly TimeSpan _window;

    public RedisRateLimiter(
        IDatabase database,
        string partitionKey,
        int permitLimit,
        TimeSpan window)
    {
        _database = database;
        _partitionKey = $"ratelimit:{partitionKey}";
        _permitLimit = permitLimit;
        _window = window;
    }

    public override TimeSpan? IdleDuration => null;

    public override RateLimiterStatistics? GetStatistics()
    {
        // Return current statistics for monitoring
        return new RateLimiterStatistics
        {
            CurrentAvailablePermits = _permitLimit,
            CurrentQueuedCount = 0,
            TotalFailedLeases = 0,
            TotalSuccessfulLeases = 0
        };
    }

    protected override RateLimitLease AttemptAcquireCore(int permitCount)
    {
        // Use Redis INCR and EXPIRE for atomic rate limiting
        var key = $"{_partitionKey}:{GetCurrentWindow()}";

        // Increment counter atomically
        var currentCount = _database.StringIncrement(key);

        // Set expiration on first increment
        if (currentCount == 1)
        {
            _database.KeyExpire(key, _window);
        }

        var isAllowed = currentCount <= _permitLimit;

        return isAllowed
            ? new RedisRateLimitLease(isAllowed: true)
            : new RedisRateLimitLease(
                isAllowed: false,
                retryAfter: GetRetryAfter());
    }

    protected override ValueTask<RateLimitLease> AcquireAsyncCore(
        int permitCount,
        CancellationToken cancellationToken)
    {
        return new ValueTask<RateLimitLease>(AttemptAcquireCore(permitCount));
    }

    private long GetCurrentWindow()
    {
        // Get current time window (e.g., for 1-minute window: timestamp / 60)
        var windowTicks = _window.Ticks;
        var currentTicks = DateTimeOffset.UtcNow.Ticks;
        return currentTicks / windowTicks;
    }

    private TimeSpan GetRetryAfter()
    {
        // Calculate when the current window expires
        var currentWindow = GetCurrentWindow();
        var nextWindow = (currentWindow + 1) * _window.Ticks;
        var now = DateTimeOffset.UtcNow.Ticks;
        return TimeSpan.FromTicks(nextWindow - now);
    }

    protected override void Dispose(bool disposing)
    {
        // Nothing to dispose
    }

    protected override ValueTask DisposeAsyncCore()
    {
        return default;
    }
}

/// <summary>
/// Rate limit lease returned by RedisRateLimiter
/// </summary>
public class RedisRateLimitLease : RateLimitLease
{
    private readonly bool _isAllowed;
    private readonly TimeSpan? _retryAfter;

    public RedisRateLimitLease(bool isAllowed, TimeSpan? retryAfter = null)
    {
        _isAllowed = isAllowed;
        _retryAfter = retryAfter;
    }

    public override bool IsAcquired => _isAllowed;

    public override IEnumerable<string> MetadataNames => _retryAfter.HasValue
        ? new[] { MetadataName.RetryAfter.Name }
        : Array.Empty<string>();

    public override bool TryGetMetadata(string metadataName, out object? metadata)
    {
        if (metadataName == MetadataName.RetryAfter.Name && _retryAfter.HasValue)
        {
            metadata = _retryAfter.Value;
            return true;
        }

        metadata = null;
        return false;
    }

    protected override void Dispose(bool disposing)
    {
        // Nothing to dispose
    }
}

using System.Collections.Concurrent;

namespace RideAway.API.Middleware;

/// <summary>
/// Fixed-window, per-client rate limiter. Hand-rolled because the framework limiter
/// only ships from .NET 7 and this project targets .NET 6.
/// </summary>
public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimitPolicy _strictPolicy;
    private readonly RateLimitPolicy _globalPolicy;
    private readonly ConcurrentDictionary<string, WindowCounter> _counters = new();

    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger,
        Microsoft.Extensions.Configuration.IConfiguration configuration)
    {
        _next = next;
        _logger = logger;

        _strictPolicy = RateLimitPolicy.FromConfiguration(configuration, "RateLimiting:Strict");
        _globalPolicy = RateLimitPolicy.FromConfiguration(configuration, "RateLimiting:Global");
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var client = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        if (_globalPolicy.IsEnabled && !TryAcquire(client, "global", _globalPolicy))
        {
            await RejectAsync(context, _globalPolicy);
            return;
        }

        // Credential endpoints get a much tighter budget than the rest of the API.
        if (_strictPolicy.IsEnabled && IsStrictEndpoint(context.Request) && !TryAcquire(client, "strict", _strictPolicy))
        {
            _logger.LogWarning("Rate limit exceeded for {Client} on {Path}", client, context.Request.Path);
            await RejectAsync(context, _strictPolicy);
            return;
        }

        await _next(context);
    }

    /// <summary>Login and registration. Everything else only has the global budget.</summary>
    private static bool IsStrictEndpoint(HttpRequest request)
    {
        var path = request.Path;

        if (path.StartsWithSegments("/api/auth", StringComparison.OrdinalIgnoreCase))
            return true;

        return HttpMethods.IsPost(request.Method)
            && (path.Equals("/api/User", StringComparison.OrdinalIgnoreCase)
                || path.Equals("/api/users", StringComparison.OrdinalIgnoreCase));
    }

    private bool TryAcquire(string client, string policyName, RateLimitPolicy policy)
    {
        // Separate counter per policy, or the budgets share one allowance.
        var counter = _counters.GetOrAdd($"{policyName}:{client}", _ => new WindowCounter());

        return counter.TryAcquire(policy.PermitLimit, policy.Window);
    }

    private static async Task RejectAsync(HttpContext context, RateLimitPolicy policy)
    {
        context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.Response.Headers["Retry-After"] = ((int)policy.Window.TotalSeconds).ToString();
        context.Response.ContentType = "application/json";

        await context.Response.WriteAsync(
            "{\"success\":false,\"message\":\"Too many requests. Please try again later.\"}");
    }

    private sealed class WindowCounter
    {
        private readonly object _gate = new();
        private DateTime _windowStart = DateTime.UtcNow;
        private int _count;

        public bool TryAcquire(int permitLimit, TimeSpan window)
        {
            lock (_gate)
            {
                var now = DateTime.UtcNow;

                if (now - _windowStart >= window)
                {
                    _windowStart = now;
                    _count = 0;
                }

                if (_count >= permitLimit)
                    return false;

                _count++;
                return true;
            }
        }
    }

    private sealed class RateLimitPolicy
    {
        public bool IsEnabled { get; private init; }
        public int PermitLimit { get; private init; }
        public TimeSpan Window { get; private init; }

        public static RateLimitPolicy FromConfiguration(Microsoft.Extensions.Configuration.IConfiguration configuration, string section)
        {
            var enabled = configuration.GetValue($"{section}:Enabled", true);

            return new RateLimitPolicy
            {
                IsEnabled = enabled,
                PermitLimit = Math.Max(1, configuration.GetValue($"{section}:PermitLimit", 100)),
                Window = TimeSpan.FromSeconds(Math.Max(1, configuration.GetValue($"{section}:WindowSeconds", 60)))
            };
        }
    }
}

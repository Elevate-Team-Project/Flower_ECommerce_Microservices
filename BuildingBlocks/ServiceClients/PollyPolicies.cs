using Polly;
using Polly.Extensions.Http;
using Polly.Timeout;

namespace BuildingBlocks.ServiceClients;

/// <summary>
/// Centralized Polly resiliency policies for HTTP communication.
/// </summary>
public static class PollyPolicies
{
    /// <summary>
    /// Creates a retry policy with exponential backoff.
    /// Retries on transient HTTP errors (5xx, 408) and timeout exceptions.
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(int retryCount = 3)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .WaitAndRetryAsync(
                retryCount,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryAttempt, context) =>
                {
                    // Log retry attempt (can be extended with ILogger)
                    Console.WriteLine($"[Polly] Retry {retryAttempt} after {timespan.TotalSeconds}s due to: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
                });
    }

    /// <summary>
    /// Creates a circuit breaker policy.
    /// Opens after specified number of consecutive failures and stays open for specified duration.
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(
        int exceptionsBeforeBreaking = 5,
        int durationOfBreakSeconds = 30)
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .Or<TimeoutRejectedException>()
            .CircuitBreakerAsync(
                exceptionsBeforeBreaking,
                TimeSpan.FromSeconds(durationOfBreakSeconds),
                onBreak: (outcome, breakDelay) =>
                {
                    Console.WriteLine($"[Polly] Circuit OPEN for {breakDelay.TotalSeconds}s due to: {outcome.Exception?.Message ?? outcome.Result?.StatusCode.ToString()}");
                },
                onReset: () =>
                {
                    Console.WriteLine("[Polly] Circuit CLOSED - normal operation resumed");
                },
                onHalfOpen: () =>
                {
                    Console.WriteLine("[Polly] Circuit HALF-OPEN - testing if service recovered");
                });
    }

    /// <summary>
    /// Creates a timeout policy per request.
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetTimeoutPolicy(int timeoutSeconds = 30)
    {
        return Policy.TimeoutAsync<HttpResponseMessage>(
            TimeSpan.FromSeconds(timeoutSeconds),
            TimeoutStrategy.Optimistic);
    }

    /// <summary>
    /// Creates a combined policy: Timeout -> Retry -> Circuit Breaker (wrapped in this order).
    /// The timeout applies per-request, retry wraps timeout, circuit breaker wraps all.
    /// </summary>
    public static IAsyncPolicy<HttpResponseMessage> GetCombinedPolicy(ServiceClientConfiguration config)
    {
        var timeoutPolicy = GetTimeoutPolicy(config.TimeoutSeconds);
        var retryPolicy = GetRetryPolicy(config.RetryCount);
        var circuitBreakerPolicy = GetCircuitBreakerPolicy(
            config.CircuitBreakerExceptionsBeforeBreaking,
            config.CircuitBreakerDurationSeconds);

        // Wrap order: CircuitBreaker -> Retry -> Timeout
        // This means: Each request has a timeout, retries happen if timeout/transient error,
        // circuit breaks if too many consecutive failures
        return Policy.WrapAsync(circuitBreakerPolicy, retryPolicy, timeoutPolicy);
    }
}

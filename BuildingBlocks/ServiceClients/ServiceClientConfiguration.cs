namespace BuildingBlocks.ServiceClients;

/// <summary>
/// Configuration for inter-service HTTP communication.
/// </summary>
public class ServiceClientConfiguration
{
    public const string SectionName = "ServiceClients";

    /// <summary>
    /// Base URL for Catalog Service (e.g., "http://catalog-service" or "https://localhost:5001")
    /// </summary>
    public string CatalogServiceUrl { get; set; } = string.Empty;

    /// <summary>
    /// Base URL for Cart Service
    /// </summary>
    public string CartServiceUrl { get; set; } = string.Empty;

    /// <summary>
    /// Base URL for Ordering Service
    /// </summary>
    public string OrderingServiceUrl { get; set; } = string.Empty;

    /// <summary>
    /// Base URL for Delivery Service
    /// </summary>
    public string DeliveryServiceUrl { get; set; } = string.Empty;

    /// <summary>
    /// Base URL for Auth Service
    /// </summary>
    public string AuthServiceUrl { get; set; } = string.Empty;

    /// <summary>
    /// Request timeout in seconds (default: 30)
    /// </summary>
    public int TimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Number of retry attempts for transient failures (default: 3)
    /// </summary>
    public int RetryCount { get; set; } = 3;

    /// <summary>
    /// Circuit breaker: number of exceptions before opening (default: 5)
    /// </summary>
    public int CircuitBreakerExceptionsBeforeBreaking { get; set; } = 5;

    /// <summary>
    /// Circuit breaker: duration to stay open in seconds (default: 30)
    /// </summary>
    public int CircuitBreakerDurationSeconds { get; set; } = 30;
}

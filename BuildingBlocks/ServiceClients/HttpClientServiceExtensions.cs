using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;

namespace BuildingBlocks.ServiceClients;

/// <summary>
/// Extension methods for registering HTTP service clients with DI.
/// </summary>
public static class HttpClientServiceExtensions
{
    /// <summary>
    /// Adds all inter-service HTTP clients with Polly resiliency policies.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration containing ServiceClients section.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddServiceClients(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Bind configuration
        var config = new ServiceClientConfiguration();
        configuration.GetSection(ServiceClientConfiguration.SectionName).Bind(config);
        services.Configure<ServiceClientConfiguration>(
            configuration.GetSection(ServiceClientConfiguration.SectionName));

        // Get combined Polly policy
        var combinedPolicy = PollyPolicies.GetCombinedPolicy(config);

        // Register Catalog Service Client
        if (!string.IsNullOrEmpty(config.CatalogServiceUrl))
        {
            services.AddHttpClient<ICatalogServiceClient, CatalogServiceClient>(client =>
            {
                client.BaseAddress = new Uri(config.CatalogServiceUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddPolicyHandler(combinedPolicy);
        }

        // Register Cart Service Client
        if (!string.IsNullOrEmpty(config.CartServiceUrl))
        {
            services.AddHttpClient<ICartServiceClient, CartServiceClient>(client =>
            {
                client.BaseAddress = new Uri(config.CartServiceUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddPolicyHandler(combinedPolicy);
        }

        // Register Ordering Service Client
        if (!string.IsNullOrEmpty(config.OrderingServiceUrl))
        {
            services.AddHttpClient<IOrderingServiceClient, OrderingServiceClient>(client =>
            {
                client.BaseAddress = new Uri(config.OrderingServiceUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddPolicyHandler(combinedPolicy);
        }

        // Register Delivery Service Client
        if (!string.IsNullOrEmpty(config.DeliveryServiceUrl))
        {
            services.AddHttpClient<IDeliveryServiceClient, DeliveryServiceClient>(client =>
            {
                client.BaseAddress = new Uri(config.DeliveryServiceUrl);
                client.DefaultRequestHeaders.Add("Accept", "application/json");
            })
            .AddPolicyHandler(combinedPolicy);
        }

        return services;
    }

    /// <summary>
    /// Adds a specific service client with custom configuration.
    /// </summary>
    public static IServiceCollection AddCatalogServiceClient(
        this IServiceCollection services,
        string baseUrl,
        Action<ServiceClientConfiguration>? configure = null)
    {
        var config = new ServiceClientConfiguration { CatalogServiceUrl = baseUrl };
        configure?.Invoke(config);

        services.AddHttpClient<ICatalogServiceClient, CatalogServiceClient>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .AddPolicyHandler(PollyPolicies.GetCombinedPolicy(config));

        return services;
    }

    /// <summary>
    /// Adds Cart Service client with custom configuration.
    /// </summary>
    public static IServiceCollection AddCartServiceClient(
        this IServiceCollection services,
        string baseUrl,
        Action<ServiceClientConfiguration>? configure = null)
    {
        var config = new ServiceClientConfiguration { CartServiceUrl = baseUrl };
        configure?.Invoke(config);

        services.AddHttpClient<ICartServiceClient, CartServiceClient>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .AddPolicyHandler(PollyPolicies.GetCombinedPolicy(config));

        return services;
    }

    /// <summary>
    /// Adds Ordering Service client with custom configuration.
    /// </summary>
    public static IServiceCollection AddOrderingServiceClient(
        this IServiceCollection services,
        string baseUrl,
        Action<ServiceClientConfiguration>? configure = null)
    {
        var config = new ServiceClientConfiguration { OrderingServiceUrl = baseUrl };
        configure?.Invoke(config);

        services.AddHttpClient<IOrderingServiceClient, OrderingServiceClient>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .AddPolicyHandler(PollyPolicies.GetCombinedPolicy(config));

        return services;
    }

    /// <summary>
    /// Adds Delivery Service client with custom configuration.
    /// </summary>
    public static IServiceCollection AddDeliveryServiceClient(
        this IServiceCollection services,
        string baseUrl,
        Action<ServiceClientConfiguration>? configure = null)
    {
        var config = new ServiceClientConfiguration { DeliveryServiceUrl = baseUrl };
        configure?.Invoke(config);

        services.AddHttpClient<IDeliveryServiceClient, DeliveryServiceClient>(client =>
        {
            client.BaseAddress = new Uri(baseUrl);
            client.DefaultRequestHeaders.Add("Accept", "application/json");
        })
        .AddPolicyHandler(PollyPolicies.GetCombinedPolicy(config));

        return services;
    }
}

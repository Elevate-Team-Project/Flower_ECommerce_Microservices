using System.Net.Http.Json;
using System.Text.Json;

namespace BuildingBlocks.ServiceClients;

/// <summary>
/// HTTP client implementation for Catalog Service.
/// </summary>
public class CatalogServiceClient : ICatalogServiceClient
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CatalogServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse<ProductDto>> GetProductByIdAsync(int productId, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/products/{productId}", ct);
            
            if (response.IsSuccessStatusCode)
            {
                var product = await response.Content.ReadFromJsonAsync<ProductDto>(JsonOptions, ct);
                return ApiResponse<ProductDto>.Success(product!);
            }

            var errorContent = await response.Content.ReadAsStringAsync(ct);
            return ApiResponse<ProductDto>.Failure(errorContent, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return ApiResponse<ProductDto>.Failure($"Failed to get product: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<ProductDto>>> GetProductsByIdsAsync(
        IEnumerable<int> productIds, 
        CancellationToken ct = default)
    {
        try
        {
            var idsQuery = string.Join(",", productIds);
            var response = await _httpClient.GetAsync($"api/products/by-ids?ids={idsQuery}", ct);
            
            if (response.IsSuccessStatusCode)
            {
                var products = await response.Content.ReadFromJsonAsync<IEnumerable<ProductDto>>(JsonOptions, ct);
                return ApiResponse<IEnumerable<ProductDto>>.Success(products!);
            }

            var errorContent = await response.Content.ReadAsStringAsync(ct);
            return ApiResponse<IEnumerable<ProductDto>>.Failure(errorContent, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<ProductDto>>.Failure($"Failed to get products: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> CheckProductAvailabilityAsync(
        int productId, 
        int quantity, 
        CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(
                $"api/products/{productId}/availability?quantity={quantity}", ct);
            
            if (response.IsSuccessStatusCode)
            {
                var isAvailable = await response.Content.ReadFromJsonAsync<bool>(JsonOptions, ct);
                return ApiResponse<bool>.Success(isAvailable);
            }

            return ApiResponse<bool>.Failure("Product not available", (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Failure($"Failed to check availability: {ex.Message}");
        }
    }
}

/// <summary>
/// HTTP client implementation for Cart Service.
/// </summary>
public class CartServiceClient : ICartServiceClient
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public CartServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse<CartDto>> GetUserCartAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/cart/{userId}", ct);
            
            if (response.IsSuccessStatusCode)
            {
                var cart = await response.Content.ReadFromJsonAsync<CartDto>(JsonOptions, ct);
                return ApiResponse<CartDto>.Success(cart!);
            }

            var errorContent = await response.Content.ReadAsStringAsync(ct);
            return ApiResponse<CartDto>.Failure(errorContent, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return ApiResponse<CartDto>.Failure($"Failed to get cart: {ex.Message}");
        }
    }

    public async Task<ApiResponse<bool>> ClearUserCartAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/cart/{userId}", ct);
            return response.IsSuccessStatusCode 
                ? ApiResponse<bool>.Success(true) 
                : ApiResponse<bool>.Failure("Failed to clear cart", (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return ApiResponse<bool>.Failure($"Failed to clear cart: {ex.Message}");
        }
    }
}

/// <summary>
/// HTTP client implementation for Ordering Service.
/// </summary>
public class OrderingServiceClient : IOrderingServiceClient
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public OrderingServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse<OrderDto>> GetOrderByIdAsync(int orderId, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/orders/{orderId}", ct);
            
            if (response.IsSuccessStatusCode)
            {
                var order = await response.Content.ReadFromJsonAsync<OrderDto>(JsonOptions, ct);
                return ApiResponse<OrderDto>.Success(order!);
            }

            var errorContent = await response.Content.ReadAsStringAsync(ct);
            return ApiResponse<OrderDto>.Failure(errorContent, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return ApiResponse<OrderDto>.Failure($"Failed to get order: {ex.Message}");
        }
    }

    public async Task<ApiResponse<IEnumerable<OrderDto>>> GetUserOrdersAsync(string userId, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/orders/user/{userId}", ct);
            
            if (response.IsSuccessStatusCode)
            {
                var orders = await response.Content.ReadFromJsonAsync<IEnumerable<OrderDto>>(JsonOptions, ct);
                return ApiResponse<IEnumerable<OrderDto>>.Success(orders!);
            }

            var errorContent = await response.Content.ReadAsStringAsync(ct);
            return ApiResponse<IEnumerable<OrderDto>>.Failure(errorContent, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return ApiResponse<IEnumerable<OrderDto>>.Failure($"Failed to get orders: {ex.Message}");
        }
    }
}

/// <summary>
/// HTTP client implementation for Delivery Service.
/// </summary>
public class DeliveryServiceClient : IDeliveryServiceClient
{
    private readonly HttpClient _httpClient;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public DeliveryServiceClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResponse<ShipmentDto>> GetShipmentByIdAsync(int shipmentId, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/shipments/{shipmentId}", ct);
            
            if (response.IsSuccessStatusCode)
            {
                var shipment = await response.Content.ReadFromJsonAsync<ShipmentDto>(JsonOptions, ct);
                return ApiResponse<ShipmentDto>.Success(shipment!);
            }

            var errorContent = await response.Content.ReadAsStringAsync(ct);
            return ApiResponse<ShipmentDto>.Failure(errorContent, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return ApiResponse<ShipmentDto>.Failure($"Failed to get shipment: {ex.Message}");
        }
    }

    public async Task<ApiResponse<ShipmentDto>> GetShipmentByOrderIdAsync(int orderId, CancellationToken ct = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/shipments/order/{orderId}", ct);
            
            if (response.IsSuccessStatusCode)
            {
                var shipment = await response.Content.ReadFromJsonAsync<ShipmentDto>(JsonOptions, ct);
                return ApiResponse<ShipmentDto>.Success(shipment!);
            }

            var errorContent = await response.Content.ReadAsStringAsync(ct);
            return ApiResponse<ShipmentDto>.Failure(errorContent, (int)response.StatusCode);
        }
        catch (Exception ex)
        {
            return ApiResponse<ShipmentDto>.Failure($"Failed to get shipment: {ex.Message}");
        }
    }
}

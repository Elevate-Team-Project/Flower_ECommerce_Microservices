namespace BuildingBlocks.ServiceClients;

/// <summary>
/// HTTP client interface for communicating with Catalog Service.
/// </summary>
public interface ICatalogServiceClient
{
    /// <summary>
    /// Gets a product by its ID.
    /// </summary>
    Task<ApiResponse<ProductDto>> GetProductByIdAsync(int productId, CancellationToken ct = default);

    /// <summary>
    /// Gets multiple products by their IDs.
    /// </summary>
    Task<ApiResponse<IEnumerable<ProductDto>>> GetProductsByIdsAsync(IEnumerable<int> productIds, CancellationToken ct = default);

    /// <summary>
    /// Checks if a product is available with the specified quantity.
    /// </summary>
    Task<ApiResponse<bool>> CheckProductAvailabilityAsync(int productId, int quantity, CancellationToken ct = default);
}

/// <summary>
/// HTTP client interface for communicating with Cart Service.
/// </summary>
public interface ICartServiceClient
{
    /// <summary>
    /// Gets a user's cart.
    /// </summary>
    Task<ApiResponse<CartDto>> GetUserCartAsync(string userId, CancellationToken ct = default);

    /// <summary>
    /// Clears a user's cart (typically after order completion).
    /// </summary>
    Task<ApiResponse<bool>> ClearUserCartAsync(string userId, CancellationToken ct = default);
}

/// <summary>
/// HTTP client interface for communicating with Ordering Service.
/// </summary>
public interface IOrderingServiceClient
{
    /// <summary>
    /// Gets order details by order ID.
    /// </summary>
    Task<ApiResponse<OrderDto>> GetOrderByIdAsync(int orderId, CancellationToken ct = default);

    /// <summary>
    /// Gets all orders for a user.
    /// </summary>
    Task<ApiResponse<IEnumerable<OrderDto>>> GetUserOrdersAsync(string userId, CancellationToken ct = default);
}

/// <summary>
/// HTTP client interface for communicating with Delivery Service.
/// </summary>
public interface IDeliveryServiceClient
{
    /// <summary>
    /// Gets shipment details by shipment ID.
    /// </summary>
    Task<ApiResponse<ShipmentDto>> GetShipmentByIdAsync(int shipmentId, CancellationToken ct = default);

    /// <summary>
    /// Gets shipment by order ID.
    /// </summary>
    Task<ApiResponse<ShipmentDto>> GetShipmentByOrderIdAsync(int orderId, CancellationToken ct = default);
}

// Additional DTOs needed for these interfaces
public class OrderDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<OrderItemDto> Items { get; set; } = new();
}

public class OrderItemDto
{
    public int ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
}

public class ShipmentDto
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? TrackingNumber { get; set; }
    public DateTime? EstimatedDeliveryDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }
}


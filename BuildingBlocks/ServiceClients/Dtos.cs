namespace BuildingBlocks.ServiceClients;

/// <summary>
/// DTO for product information returned by Catalog Service.
/// </summary>
public class ProductDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public decimal? DiscountedPrice { get; set; }
    public int StockQuantity { get; set; }
    public bool IsAvailable { get; set; }
    public string? ImageUrl { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
}

/// <summary>
/// DTO for cart information.
/// </summary>
public class CartDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public List<CartItemDto> Items { get; set; } = new();
    public decimal TotalAmount { get; set; }
}

/// <summary>
/// DTO for cart item.
/// </summary>
public class CartItemDto
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
}

/// <summary>
/// Generic API response wrapper.
/// </summary>
public class ApiResponse<T>
{
    public bool IsSuccess { get; set; }
    public T? Data { get; set; }
    public string? ErrorMessage { get; set; }
    public int StatusCode { get; set; }

    public static ApiResponse<T> Success(T data) => new()
    {
        IsSuccess = true,
        Data = data,
        StatusCode = 200
    };

    public static ApiResponse<T> Failure(string error, int statusCode = 500) => new()
    {
        IsSuccess = false,
        ErrorMessage = error,
        StatusCode = statusCode
    };
}

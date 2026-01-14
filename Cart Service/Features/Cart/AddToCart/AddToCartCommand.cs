using MediatR;
using Cart_Service.Features.Shared;

namespace Cart_Service.Features.Cart.AddToCart
{
    /// <summary>
    /// US-D01: Add product to cart
    /// </summary>
    public record AddToCartCommand(
        string UserId,
        int ProductId,
        int Quantity = 1
    ) : IRequest<EndpointResponse<CartItemDto>>;

    public record CartItemDto(
        int Id,
        int ProductId,
        string ProductName,
        decimal UnitPrice,
        int Quantity,
        string? PictureUrl,
        decimal TotalPrice
    );
}

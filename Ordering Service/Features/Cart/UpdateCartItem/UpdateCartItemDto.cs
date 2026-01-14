namespace Ordering_Service.Features.Cart.UpdateCartItem
{
    public class UpdateCartItemDto
    {
        public int CartItemId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        
        public decimal UnitPrice { get; set; }

        public int Quantity { get; set; } = 1;
        public string? PictureUrl { get; set; }
    }
}

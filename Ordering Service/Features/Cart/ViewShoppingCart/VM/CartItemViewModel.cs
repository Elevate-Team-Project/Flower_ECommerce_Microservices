namespace Ordering_Service.Features.Cart.ViewShoppingCart.VM
{
    public class CartItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string? PictureUrl { get; set; }

        public decimal ItemTotal => UnitPrice * Quantity;
    }
}

namespace Ordering_Service.Features.Cart.ViewShoppingCart.VM
{
    public class CartViewModel
    {
        public List<CartItemViewModel> Items { get; set; } = new();
        public decimal Subtotal { get; set; }
        public decimal DeliveryFee { get; set; }
        public decimal Total => Subtotal + DeliveryFee;
    }
}

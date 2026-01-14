namespace Ordering_Service.Features.Orders.ViewMyOrders.ViewModels
{
    public class OrderItemViewModel
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public string? ProductImageUrl { get; set; }
    }
}

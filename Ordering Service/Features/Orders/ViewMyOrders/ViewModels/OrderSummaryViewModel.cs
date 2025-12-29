namespace Ordering_Service.Features.Orders.ViewMyOrders.ViewModels
{
    public class OrderSummaryViewModel
    {
        public int OrderId { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }

        public decimal TotalAmount { get; set; }

        public List<OrderItemViewModel> Items { get; set; } = new();
    }
}

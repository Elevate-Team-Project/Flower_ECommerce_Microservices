namespace Ordering_Service.Features.Orders
{
    public class OrderStatusHistoryDto
    {
        public string Status { get; set; }
        public DateTime ChangedAt { get; set; }
        public string ChangedBy { get; set; }
    }
}

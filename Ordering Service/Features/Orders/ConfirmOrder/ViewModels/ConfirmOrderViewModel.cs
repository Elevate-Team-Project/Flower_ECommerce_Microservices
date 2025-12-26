namespace Ordering_Service.Features.Orders.ConfirmOrder.ViewModels
{
    public class ConfirmOrderViewModel
    {
        public int OrderId { get; set; }

        public string ShippingAddress { get; set; }
        public string PaymentMethod { get; set; }

        public decimal SubTotal { get; set; }
        public decimal ShippingCost { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }

        public string Status { get; set; }

        public List<ItemViewModel> Items { get; set; }

        public string TrackUrl { get; set; }
    }
}

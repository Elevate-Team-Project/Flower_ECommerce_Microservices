namespace Ordering_Service.Features.Orders.ConfirmOrder.ViewModels
{
    public class ItemViewModel
    {
        public string ProductName { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice { get; set; }
    }
}

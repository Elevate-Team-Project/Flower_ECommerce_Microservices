using BuildingBlocks.SharedEntities;

namespace Ordering_Service.Entities
{
    public class Shipment : BaseEntity
    {
        public int OrderId { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public string Carrier { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending, InTransit, OutForDelivery, Delivered, Failed
        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public string? CurrentLocation { get; set; }
        public string? Notes { get; set; }

        // Navigation Property
        public virtual Order Order { get; set; } = null!;
    }
}

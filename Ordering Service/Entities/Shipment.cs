using BuildingBlocks.SharedEntities;

namespace Ordering_Service.Entities
{
    public class Shipment : BaseEntity
    {
        public int OrderId { get; set; }
        public int DeliveryAddressId { get; set; }
        public string TrackingNumber { get; set; } = string.Empty;
        public string Carrier { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Pending, InTransit, OutForDelivery, Delivered, Failed
        public DateTime? EstimatedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public string? CurrentLocation { get; set; }
        public string? Notes { get; set; }

        // Gift order fields
        public bool IsGift { get; set; } = false;
        public string? RecipientName { get; set; }
        public string? RecipientPhone { get; set; }
        public string? GiftMessage { get; set; }

        // Driver Location Tracking (US-E02)
        public double? DriverLatitude { get; set; }
        public double? DriverLongitude { get; set; }
        public DateTime? LastLocationUpdate { get; set; }
        
        // Delivery Hero Info (US-E02)
        public string? DriverName { get; set; }
        public string? DriverPhone { get; set; }
        public string? DriverPhotoUrl { get; set; }

        // Navigation Property
        public virtual UserAddress? DeliveryAddress { get; set; }
    }
}

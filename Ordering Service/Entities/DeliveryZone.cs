using BuildingBlocks.SharedEntities;

namespace Ordering_Service.Entities
{
    public class DeliveryZone : BaseEntity
    {
        public string ZoneName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public decimal ShippingCost { get; set; }
        public int EstimatedDeliveryDays { get; set; }
        public bool IsActive { get; set; } = true;
    }
}

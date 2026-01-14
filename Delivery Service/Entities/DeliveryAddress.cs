using BuildingBlocks.SharedEntities;

namespace Delivery_Service.Entities
{
    public class DeliveryAddress : BaseEntity
    {
        
        public string UserId { get; set; }

        public string Label { get; set; }              // Home, Work
        
        public string country { get; set; }
        public string city { get; set; }
        public string Street { get; set; }
        public string? Building { get; set; }
        public string? Floor { get; set; }
        public string ? postalCode { get; set; }
        public int ? phone { get; set; }

        public double Latitude { get; set; }
        public double Longitude { get; set; }

        public DateTime CreatedAt { get; set; }
        public string fullAddress { get; set; }      // Raw address text
    }

}

using BuildingBlocks.SharedEntities;

namespace Delivery_Service.Entities
{
    public class UserAddress : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string AddressLabel { get; set; } = string.Empty; // Home, Work, Other
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public bool IsDefault { get; set; } = false;
        public string? Notes { get; set; }

        // Computed property for full address display
        public string FullAddress => $"{Street}, {City}, {State} {PostalCode}, {Country}";
    }
}

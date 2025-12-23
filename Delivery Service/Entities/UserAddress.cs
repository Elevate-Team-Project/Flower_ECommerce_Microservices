using BuildingBlocks.SharedEntities;

namespace Delivery_Service.Entities
{
    public class UserAddress : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string AddressLabel { get; set; } = string.Empty; // Home, Work, Other
        public string FullName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;

        // Map Location (US-F03)
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        // Detailed Address Fields (Egyptian format)
        public string Governorate { get; set; } = string.Empty; // e.g., Cairo, Giza, Alexandria
        public string City { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string? Building { get; set; }
        public string? Floor { get; set; }
        public string? Apartment { get; set; }
        public string? PostalCode { get; set; }
        public string Country { get; set; } = "Egypt";

        public bool IsDefault { get; set; } = false;
        public string? Notes { get; set; }
        public string? Landmark { get; set; } // Near mosque, school, etc.

        // Computed property for full address display
        public string FullAddress => BuildFullAddress();

        private string BuildFullAddress()
        {
            var parts = new List<string>();
            
            if (!string.IsNullOrEmpty(Apartment)) parts.Add($"Apt {Apartment}");
            if (!string.IsNullOrEmpty(Floor)) parts.Add($"Floor {Floor}");
            if (!string.IsNullOrEmpty(Building)) parts.Add($"Building {Building}");
            if (!string.IsNullOrEmpty(Street)) parts.Add(Street);
            if (!string.IsNullOrEmpty(City)) parts.Add(City);
            if (!string.IsNullOrEmpty(Governorate)) parts.Add(Governorate);
            if (!string.IsNullOrEmpty(Country)) parts.Add(Country);

            return string.Join(", ", parts);
        }
    }
}

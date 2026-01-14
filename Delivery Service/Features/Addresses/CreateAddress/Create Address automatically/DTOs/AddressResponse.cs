namespace Delivery_Service.Features.Addresses.CreateAddress.Create_Address_automatically.DTOs
{
    public class AddressResponse
    {
        public int Id { get; set; }
        public string Label { get; set; }
        public string postalCode { get; set; }
        public string FormattedAddress { get; set; }

        public LocationDto Location { get; set; }
    }

    

}

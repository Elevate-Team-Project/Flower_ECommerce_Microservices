namespace Delivery_Service.Features.Addresses.CreateAddress.Create_Address_automatically.DTOs
{
    public class CreateAddressRequest { 
    public int UserId { get; set; }
    public string Label { get; set; }
    public int phonenumer { get;set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
  }
}

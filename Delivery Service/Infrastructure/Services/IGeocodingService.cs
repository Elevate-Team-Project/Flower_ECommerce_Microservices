using Delivery_Service.Features.Addresses.CreateAddress;
using Delivery_Service.Features.Addresses.CreateAddress.Create_Address_automatically.DTOs;

namespace Delivery_Service.Infrastructure.Services
{
    public interface IGeocodingService
    {
        
        Task<AddressAutoDto> GetAddressFromCoordinates(double latitude, double longitude);
    }

}

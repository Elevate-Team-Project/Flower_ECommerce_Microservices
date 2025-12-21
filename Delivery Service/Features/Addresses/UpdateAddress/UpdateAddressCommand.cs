using MediatR;
using Delivery_Service.Features.Shared;
using Delivery_Service.Features.Addresses.CreateAddress;

namespace Delivery_Service.Features.Addresses.UpdateAddress
{
    public record UpdateAddressCommand(
        int AddressId,
        string UserId,
        string AddressLabel,
        string FullName,
        string Phone,
        // Map Location
        double? Latitude,
        double? Longitude,
        // Address Details
        string Governorate,
        string City,
        string Street,
        string? Building,
        string? Floor,
        string? Apartment,
        string? PostalCode,
        string Country = "Egypt",
        string? Notes = null,
        string? Landmark = null
    ) : IRequest<EndpointResponse<AddressDto>>;
}

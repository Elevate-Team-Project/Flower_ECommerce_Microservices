using MediatR;
using Ordering_Service.Features.Delivery.Shared;
using Ordering_Service.Features.Delivery.Addresses.CreateAddress;

namespace Ordering_Service.Features.Delivery.Addresses.UpdateAddress
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

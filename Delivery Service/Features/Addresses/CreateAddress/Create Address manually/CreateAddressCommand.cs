using MediatR;
using Delivery_Service.Features.Shared;

namespace Delivery_Service.Features.Addresses.CreateAddress
{
    public record CreateAddressCommand(
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
        bool IsDefault = false,
        string? Notes = null,
        string? Landmark = null
    ) : IRequest<EndpointResponse<AddressDto>>;

    public record AddressDto(
        int Id,
        string? AddressLabel,
        string? FullName,
        string? Phone,
        // Map Location
        double? Latitude,
        double? Longitude,
        // Address Details
        string? Governorate,
        string? City,
        string ?Street,
        string? Building,
        string? Floor,
        string? Apartment,
        string? PostalCode,
        string Country,
        bool ?IsDefault,
        string? Landmark,
        string? FullAddress
    );
}

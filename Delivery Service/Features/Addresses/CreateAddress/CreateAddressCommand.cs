using MediatR;
using Delivery_Service.Features.Shared;

namespace Delivery_Service.Features.Addresses.CreateAddress
{
    public record CreateAddressCommand(
        string UserId,
        string AddressLabel,
        string FullName,
        string Phone,
        string Street,
        string City,
        string State,
        string PostalCode,
        string Country,
        bool IsDefault = false,
        string? Notes = null
    ) : IRequest<EndpointResponse<AddressDto>>;

    public record AddressDto(
        int Id,
        string AddressLabel,
        string FullName,
        string Phone,
        string Street,
        string City,
        string State,
        string PostalCode,
        string Country,
        bool IsDefault,
        string FullAddress
    );
}

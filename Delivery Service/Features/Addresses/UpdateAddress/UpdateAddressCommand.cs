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
        string Street,
        string City,
        string State,
        string PostalCode,
        string Country,
        string? Notes = null
    ) : IRequest<EndpointResponse<AddressDto>>;
}

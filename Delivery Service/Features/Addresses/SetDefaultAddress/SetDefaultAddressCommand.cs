using MediatR;
using Delivery_Service.Features.Shared;

namespace Delivery_Service.Features.Addresses.SetDefaultAddress
{
    public record SetDefaultAddressCommand(int AddressId, string UserId) : IRequest<EndpointResponse<bool>>;
}

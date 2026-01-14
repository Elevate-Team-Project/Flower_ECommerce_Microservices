using MediatR;
using Ordering_Service.Features.Delivery.Shared;

namespace Ordering_Service.Features.Delivery.Addresses.SetDefaultAddress
{
    public record SetDefaultAddressCommand(int AddressId, string UserId) : IRequest<EndpointResponse<bool>>;
}

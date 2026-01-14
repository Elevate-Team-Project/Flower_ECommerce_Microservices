using MediatR;
using Ordering_Service.Features.Delivery.Shared;

namespace Ordering_Service.Features.Delivery.Addresses.DeleteAddress
{
    public record DeleteAddressCommand(int AddressId, string UserId) : IRequest<EndpointResponse<bool>>;
}

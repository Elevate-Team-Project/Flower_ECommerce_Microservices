using MediatR;
using Delivery_Service.Features.Shared;

namespace Delivery_Service.Features.Addresses.DeleteAddress
{
    public record DeleteAddressCommand(int AddressId, string UserId) : IRequest<EndpointResponse<bool>>;
}

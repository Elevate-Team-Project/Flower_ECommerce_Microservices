using MediatR;
using Ordering_Service.Features.Delivery.Shared;
using Ordering_Service.Features.Delivery.Addresses.CreateAddress;

namespace Ordering_Service.Features.Delivery.Addresses.GetUserAddresses
{
    public record GetUserAddressesQuery(string UserId) : IRequest<EndpointResponse<List<AddressDto>>>;
}

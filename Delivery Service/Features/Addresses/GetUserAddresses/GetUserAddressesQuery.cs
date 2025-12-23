using MediatR;
using Delivery_Service.Features.Shared;
using Delivery_Service.Features.Addresses.CreateAddress;

namespace Delivery_Service.Features.Addresses.GetUserAddresses
{
    public record GetUserAddressesQuery(string UserId) : IRequest<EndpointResponse<List<AddressDto>>>;
}

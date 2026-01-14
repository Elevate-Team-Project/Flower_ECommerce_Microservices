using Delivery_Service.Features.Addresses.CreateAddress.Create_Address_automatically.DTOs;
using Delivery_Service.Features.Shared;
using MediatR;

namespace Delivery_Service.Features.Addresses.CreateAddress.Create_Address_automatically
{
    public record CreateAddressAutomaticallyCommand(CreateAddressRequest CreateAddressRequest) : IRequest<RequestResponse<AddressResponse>>;
    
}

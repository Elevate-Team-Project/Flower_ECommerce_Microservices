using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Delivery_Service.Entities;
using Delivery_Service.Features.Shared;
using Delivery_Service.Features.Addresses.CreateAddress;

namespace Delivery_Service.Features.Addresses.GetUserAddresses
{
    public class GetUserAddressesHandler : IRequestHandler<GetUserAddressesQuery, EndpointResponse<List<AddressDto>>>
    {
        private readonly IBaseRepository<UserAddress> _addressRepository;

        public GetUserAddressesHandler(IBaseRepository<UserAddress> addressRepository)
            => _addressRepository = addressRepository;

        public async Task<EndpointResponse<List<AddressDto>>> Handle(GetUserAddressesQuery request, CancellationToken cancellationToken)
        {
            var addresses = await _addressRepository.GetAll()
                .Where(a => a.UserId == request.UserId)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.CreatedAt)
                .Select(a => new AddressDto(
                    a.Id, a.AddressLabel, a.FullName, a.Phone,
                    a.Street, a.City, a.State, a.PostalCode,
                    a.Country, a.IsDefault, a.FullAddress
                ))
                .ToListAsync(cancellationToken);

            return EndpointResponse<List<AddressDto>>.SuccessResponse(addresses, "Addresses retrieved successfully");
        }
    }
}

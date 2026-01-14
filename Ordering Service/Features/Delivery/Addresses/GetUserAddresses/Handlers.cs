using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ordering_Service.Entities;
using Ordering_Service.Features.Delivery.Shared;
using Ordering_Service.Features.Delivery.Addresses.CreateAddress;

namespace Ordering_Service.Features.Delivery.Addresses.GetUserAddresses
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
                    a.Id,
                    a.AddressLabel,
                    a.FullName,
                    a.Phone,
                    a.Latitude,
                    a.Longitude,
                    a.Governorate,
                    a.City,
                    a.Street,
                    a.Building,
                    a.Floor,
                    a.Apartment,
                    a.PostalCode,
                    a.Country,
                    a.IsDefault,
                    a.Landmark,
                    a.FullAddress
                ))
                .ToListAsync(cancellationToken);

            return EndpointResponse<List<AddressDto>>.SuccessResponse(addresses, "Addresses retrieved successfully");
        }
    }
}

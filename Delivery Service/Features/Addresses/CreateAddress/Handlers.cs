using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Delivery_Service.Entities;
using Delivery_Service.Features.Shared;

namespace Delivery_Service.Features.Addresses.CreateAddress
{
    public class CreateAddressHandler : IRequestHandler<CreateAddressCommand, EndpointResponse<AddressDto>>
    {
        private readonly IBaseRepository<UserAddress> _addressRepository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateAddressHandler(IBaseRepository<UserAddress> addressRepository, IUnitOfWork unitOfWork)
        {
            _addressRepository = addressRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<AddressDto>> Handle(CreateAddressCommand request, CancellationToken cancellationToken)
        {
            // If this is set as default, unset other defaults
            if (request.IsDefault)
            {
                var existingDefaults = await _addressRepository.GetAll()
                    .Where(a => a.UserId == request.UserId && a.IsDefault)
                    .ToListAsync(cancellationToken);

                foreach (var addr in existingDefaults)
                {
                    addr.IsDefault = false;
                    _addressRepository.Update(addr);
                }
            }

            var address = new UserAddress
            {
                UserId = request.UserId,
                AddressLabel = request.AddressLabel,
                FullName = request.FullName,
                Phone = request.Phone,
                // Map Location
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                // Address Details
                Governorate = request.Governorate,
                City = request.City,
                Street = request.Street,
                Building = request.Building,
                Floor = request.Floor,
                Apartment = request.Apartment,
                PostalCode = request.PostalCode,
                Country = request.Country,
                IsDefault = request.IsDefault,
                Notes = request.Notes,
                Landmark = request.Landmark
            };

            await _addressRepository.AddAsync(address);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = new AddressDto(
                address.Id,
                address.AddressLabel,
                address.FullName,
                address.Phone,
                address.Latitude,
                address.Longitude,
                address.Governorate,
                address.City,
                address.Street,
                address.Building,
                address.Floor,
                address.Apartment,
                address.PostalCode,
                address.Country,
                address.IsDefault,
                address.Landmark,
                address.FullAddress
            );

            return EndpointResponse<AddressDto>.SuccessResponse(dto, "Address created successfully", 201);
        }
    }
}

using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ordering_Service.Entities;
using Ordering_Service.Features.Delivery.Shared;
using Ordering_Service.Features.Delivery.Addresses.CreateAddress;

namespace Ordering_Service.Features.Delivery.Addresses.UpdateAddress
{
    public class UpdateAddressHandler : IRequestHandler<UpdateAddressCommand, EndpointResponse<AddressDto>>
    {
        private readonly IBaseRepository<UserAddress> _addressRepository;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateAddressHandler(IBaseRepository<UserAddress> addressRepository, IUnitOfWork unitOfWork)
        {
            _addressRepository = addressRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<AddressDto>> Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
        {
            var address = await _addressRepository.GetAll()
                .FirstOrDefaultAsync(a => a.Id == request.AddressId && a.UserId == request.UserId, cancellationToken);

            if (address == null)
                return EndpointResponse<AddressDto>.NotFoundResponse("Address not found");

            address.AddressLabel = request.AddressLabel;
            address.FullName = request.FullName;
            address.Phone = request.Phone;
            address.Latitude = request.Latitude;
            address.Longitude = request.Longitude;
            address.Governorate = request.Governorate;
            address.City = request.City;
            address.Street = request.Street;
            address.Building = request.Building;
            address.Floor = request.Floor;
            address.Apartment = request.Apartment;
            address.PostalCode = request.PostalCode;
            address.Country = request.Country;
            address.Notes = request.Notes;
            address.Landmark = request.Landmark;

            _addressRepository.Update(address);
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

            return EndpointResponse<AddressDto>.SuccessResponse(dto, "Address updated successfully");
        }
    }
}

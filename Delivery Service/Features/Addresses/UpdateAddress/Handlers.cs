using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Delivery_Service.Entities;
using Delivery_Service.Features.Shared;
using Delivery_Service.Features.Addresses.CreateAddress;

namespace Delivery_Service.Features.Addresses.UpdateAddress
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
            address.Street = request.Street;
            address.City = request.City;
            address.State = request.State;
            address.PostalCode = request.PostalCode;
            address.Country = request.Country;
            address.Notes = request.Notes;

            _addressRepository.Update(address);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = new AddressDto(
                address.Id, address.AddressLabel, address.FullName, address.Phone,
                address.Street, address.City, address.State, address.PostalCode,
                address.Country, address.IsDefault, address.FullAddress
            );

            return EndpointResponse<AddressDto>.SuccessResponse(dto, "Address updated successfully");
        }
    }
}

using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Delivery_Service.Entities;
using Delivery_Service.Features.Shared;

namespace Delivery_Service.Features.Addresses.SetDefaultAddress
{
    public class SetDefaultAddressHandler : IRequestHandler<SetDefaultAddressCommand, EndpointResponse<bool>>
    {
        private readonly IBaseRepository<UserAddress> _addressRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SetDefaultAddressHandler(IBaseRepository<UserAddress> addressRepository, IUnitOfWork unitOfWork)
        {
            _addressRepository = addressRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<bool>> Handle(SetDefaultAddressCommand request, CancellationToken cancellationToken)
        {
            var address = await _addressRepository.GetAll()
                .FirstOrDefaultAsync(a => a.Id == request.AddressId && a.UserId == request.UserId, cancellationToken);

            if (address == null)
                return EndpointResponse<bool>.NotFoundResponse("Address not found");

            // Unset other defaults
            var existingDefaults = await _addressRepository.GetAll()
                .Where(a => a.UserId == request.UserId && a.IsDefault && a.Id != request.AddressId)
                .ToListAsync(cancellationToken);

            foreach (var addr in existingDefaults)
            {
                addr.IsDefault = false;
                _addressRepository.Update(addr);
            }

            address.IsDefault = true;
            _addressRepository.Update(address);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return EndpointResponse<bool>.SuccessResponse(true, "Default address set successfully");
        }
    }
}

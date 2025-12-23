using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Delivery_Service.Entities;
using Delivery_Service.Features.Shared;

namespace Delivery_Service.Features.Addresses.DeleteAddress
{
    public class DeleteAddressHandler : IRequestHandler<DeleteAddressCommand, EndpointResponse<bool>>
    {
        private readonly IBaseRepository<UserAddress> _addressRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteAddressHandler(IBaseRepository<UserAddress> addressRepository, IUnitOfWork unitOfWork)
        {
            _addressRepository = addressRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<bool>> Handle(DeleteAddressCommand request, CancellationToken cancellationToken)
        {
            var address = await _addressRepository.GetAll()
                .FirstOrDefaultAsync(a => a.Id == request.AddressId && a.UserId == request.UserId, cancellationToken);

            if (address == null)
                return EndpointResponse<bool>.NotFoundResponse("Address not found");

            _addressRepository.Delete(address);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return EndpointResponse<bool>.SuccessResponse(true, "Address deleted successfully");
        }
    }
}

using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Promotion_Service.Entities;
using Promotion_Service.Features.Shared;

namespace Promotion_Service.Features.Offers.DeleteOffer
{
    public class DeleteOfferHandler : IRequestHandler<DeleteOfferCommand, EndpointResponse<bool>>
    {
        private readonly IBaseRepository<Offer> _offerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DeleteOfferHandler> _logger;

        public DeleteOfferHandler(
            IBaseRepository<Offer> offerRepository,
            IUnitOfWork unitOfWork,
            ILogger<DeleteOfferHandler> logger)
        {
            _offerRepository = offerRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<EndpointResponse<bool>> Handle(
            DeleteOfferCommand request,
            CancellationToken cancellationToken)
        {
            var offer = await _offerRepository.GetAll()
                .FirstOrDefaultAsync(o => o.Id == request.OfferId, cancellationToken);

            if (offer == null)
                return EndpointResponse<bool>.NotFoundResponse("Offer not found");

            // Soft delete
            _offerRepository.Delete(offer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Deleted offer {OfferId}: {OfferName}", offer.Id, offer.Name);

            // TODO: Publish event to notify Catalog Service to recalculate prices

            return EndpointResponse<bool>.SuccessResponse(true, "Offer deleted successfully");
        }
    }
}

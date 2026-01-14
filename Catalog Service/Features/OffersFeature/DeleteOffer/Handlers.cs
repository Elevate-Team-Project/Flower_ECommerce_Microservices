using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Catalog_Service.Entities;
using Catalog_Service.Features.Shared;

namespace Catalog_Service.Features.OffersFeature.DeleteOffer
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

            // TODO: Publish event to notify other services if needed, though Catalog logic is internal now.
            // Maybe republish ProductLowStockEvent or PriceChangedEvent? 
            // For now just deleting.

            return EndpointResponse<bool>.SuccessResponse(true, "Offer deleted successfully");
        }
    }
}

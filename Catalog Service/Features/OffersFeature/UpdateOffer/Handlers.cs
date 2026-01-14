using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Catalog_Service.Entities;
using Catalog_Service.Features.OffersFeature.CreateOffer;
using Catalog_Service.Features.Shared;

namespace Catalog_Service.Features.OffersFeature.UpdateOffer
{
    public class UpdateOfferHandler : IRequestHandler<UpdateOfferCommand, EndpointResponse<OfferDto>>
    {
        private readonly IBaseRepository<Offer> _offerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<UpdateOfferHandler> _logger;

        public UpdateOfferHandler(
            IBaseRepository<Offer> offerRepository,
            IUnitOfWork unitOfWork,
            ILogger<UpdateOfferHandler> logger)
        {
            _offerRepository = offerRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<EndpointResponse<OfferDto>> Handle(
            UpdateOfferCommand request,
            CancellationToken cancellationToken)
        {
            var offer = await _offerRepository.GetAll()
                .FirstOrDefaultAsync(o => o.Id == request.OfferId, cancellationToken);

            if (offer == null)
                return EndpointResponse<OfferDto>.NotFoundResponse("Offer not found");

            // Update properties
            offer.Name = request.Name;
            offer.NameAr = request.NameAr;
            offer.Description = request.Description;
            offer.DescriptionAr = request.DescriptionAr;
            offer.Type = request.Type;
            offer.DiscountValue = request.DiscountValue;
            offer.MaxDiscountAmount = request.MaxDiscountAmount;
            offer.TargetType = request.TargetType;
            offer.ProductId = request.ProductId;
            offer.CategoryId = request.CategoryId;
            offer.OccasionId = request.OccasionId;
            offer.StartDate = request.StartDate;
            offer.EndDate = request.EndDate;
            offer.Priority = request.Priority;
            offer.AdminNotes = request.AdminNotes;

            // Update status based on dates
            var now = DateTime.UtcNow;
            if (now > request.EndDate)
                offer.Status = OfferStatus.Expired;
            else if (now < request.StartDate)
                offer.Status = OfferStatus.Scheduled;
            else
                offer.Status = OfferStatus.Active;

            _offerRepository.Update(offer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Updated offer {OfferId}: {OfferName}", offer.Id, offer.Name);

            var dto = new OfferDto(
                offer.Id,
                offer.Name,
                offer.NameAr,
                offer.Description,
                offer.Type,
                offer.DiscountValue,
                offer.MaxDiscountAmount,
                offer.TargetType,
                offer.ProductId,
                offer.CategoryId,
                offer.OccasionId,
                offer.StartDate,
                offer.EndDate,
                offer.Status,
                offer.Priority,
                offer.IsActive,
                offer.CreatedAt
            );

            return EndpointResponse<OfferDto>.SuccessResponse(dto, "Offer updated successfully");
        }
    }
}

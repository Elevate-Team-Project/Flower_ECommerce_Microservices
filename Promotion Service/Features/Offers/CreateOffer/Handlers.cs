using BuildingBlocks.Interfaces;
using MediatR;
using Promotion_Service.Entities;
using Promotion_Service.Features.Shared;

namespace Promotion_Service.Features.Offers.CreateOffer
{
    public class CreateOfferHandler : IRequestHandler<CreateOfferCommand, EndpointResponse<OfferDto>>
    {
        private readonly IBaseRepository<Offer> _offerRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CreateOfferHandler> _logger;

        public CreateOfferHandler(
            IBaseRepository<Offer> offerRepository,
            IUnitOfWork unitOfWork,
            ILogger<CreateOfferHandler> logger)
        {
            _offerRepository = offerRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<EndpointResponse<OfferDto>> Handle(
            CreateOfferCommand request,
            CancellationToken cancellationToken)
        {
            // Determine initial status
            var now = DateTime.UtcNow;
            var status = OfferStatus.Scheduled;
            if (now >= request.StartDate && now <= request.EndDate)
            {
                status = OfferStatus.Active;
            }

            var offer = new Offer
            {
                Name = request.Name,
                NameAr = request.NameAr,
                Description = request.Description,
                DescriptionAr = request.DescriptionAr,
                Type = request.Type,
                DiscountValue = request.DiscountValue,
                MaxDiscountAmount = request.MaxDiscountAmount,
                TargetType = request.TargetType,
                ProductId = request.ProductId,
                CategoryId = request.CategoryId,
                OccasionId = request.OccasionId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                Status = status,
                Priority = request.Priority,
                AdminNotes = request.AdminNotes,
                IsActive = true
            };

            await _offerRepository.AddAsync(offer);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Created offer {OfferId}: {OfferName}", offer.Id, offer.Name);

            var dto = MapToDto(offer);
            return EndpointResponse<OfferDto>.SuccessResponse(dto, "Offer created successfully", 201);
        }

        private static OfferDto MapToDto(Offer offer) => new(
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
    }
}

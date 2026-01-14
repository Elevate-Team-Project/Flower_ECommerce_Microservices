using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Promotion_Service.Entities;
using Promotion_Service.Features.Offers.CreateOffer;
using Promotion_Service.Features.Shared;

namespace Promotion_Service.Features.Offers.GetOfferById
{
    public class GetOfferByIdHandler : IRequestHandler<GetOfferByIdQuery, EndpointResponse<OfferDto>>
    {
        private readonly IBaseRepository<Offer> _offerRepository;

        public GetOfferByIdHandler(IBaseRepository<Offer> offerRepository)
        {
            _offerRepository = offerRepository;
        }

        public async Task<EndpointResponse<OfferDto>> Handle(
            GetOfferByIdQuery request,
            CancellationToken cancellationToken)
        {
            var offer = await _offerRepository.GetAll()
                .FirstOrDefaultAsync(o => o.Id == request.OfferId, cancellationToken);

            if (offer == null)
                return EndpointResponse<OfferDto>.NotFoundResponse("Offer not found");

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

            return EndpointResponse<OfferDto>.SuccessResponse(dto, "Offer retrieved successfully");
        }
    }
}

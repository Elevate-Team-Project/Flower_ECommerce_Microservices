using MediatR;
using Promotion_Service.Entities;
using Promotion_Service.Features.Offers.CreateOffer;
using Promotion_Service.Features.Shared;

namespace Promotion_Service.Features.Offers.UpdateOffer
{
    /// <summary>
    /// US-G03: Edit Offer
    /// </summary>
    public record UpdateOfferCommand(
        int OfferId,
        string Name,
        string? NameAr,
        string? Description,
        string? DescriptionAr,
        OfferType Type,
        decimal DiscountValue,
        decimal? MaxDiscountAmount,
        OfferTargetType TargetType,
        int? ProductId,
        int? CategoryId,
        int? OccasionId,
        DateTime StartDate,
        DateTime EndDate,
        int Priority = 0,
        string? AdminNotes = null
    ) : IRequest<EndpointResponse<OfferDto>>;
}

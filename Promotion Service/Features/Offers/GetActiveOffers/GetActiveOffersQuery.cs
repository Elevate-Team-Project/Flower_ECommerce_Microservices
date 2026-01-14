using MediatR;
using Promotion_Service.Entities;
using Promotion_Service.Features.Shared;

namespace Promotion_Service.Features.Offers.GetActiveOffers
{
    /// <summary>
    /// US-G05: Display Offers to Customer
    /// </summary>
    public record GetActiveOffersQuery(
        int? ProductId = null,
        int? CategoryId = null,
        int? OccasionId = null
    ) : IRequest<EndpointResponse<List<ActiveOfferDto>>>;

    public record ActiveOfferDto(
        int Id,
        string Name,
        string? NameAr,
        string? Description,
        OfferType Type,
        decimal DiscountValue,
        decimal? MaxDiscountAmount,
        OfferTargetType TargetType,
        int? ProductId,
        int? CategoryId,
        int? OccasionId,
        DateTime EndDate
    );
}

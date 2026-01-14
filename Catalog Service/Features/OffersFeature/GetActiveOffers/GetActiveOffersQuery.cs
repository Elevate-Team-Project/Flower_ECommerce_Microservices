using MediatR;
using Catalog_Service.Entities;
using Catalog_Service.Features.Shared;

namespace Catalog_Service.Features.OffersFeature.GetActiveOffers
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

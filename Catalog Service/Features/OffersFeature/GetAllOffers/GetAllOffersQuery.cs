using MediatR;
using Catalog_Service.Features.OffersFeature.CreateOffer;
using Catalog_Service.Features.Shared;

namespace Catalog_Service.Features.OffersFeature.GetAllOffers
{
    /// <summary>
    /// US-G02: View All Offers
    /// </summary>
    public record GetAllOffersQuery(
        string? Status = null,  // Filter: active, scheduled, expired
        string? SortBy = "status"  // Sort by: status, name, startDate, endDate
    ) : IRequest<EndpointResponse<List<OfferDto>>>;
}

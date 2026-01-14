using MediatR;
using Promotion_Service.Features.Offers.CreateOffer;
using Promotion_Service.Features.Shared;

namespace Promotion_Service.Features.Offers.GetAllOffers
{
    /// <summary>
    /// US-G02: View All Offers
    /// </summary>
    public record GetAllOffersQuery(
        string? Status = null,  // Filter: active, scheduled, expired
        string? SortBy = "status"  // Sort by: status, name, startDate, endDate
    ) : IRequest<EndpointResponse<List<OfferDto>>>;
}

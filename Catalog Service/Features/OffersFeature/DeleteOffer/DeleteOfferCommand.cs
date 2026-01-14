using MediatR;
using Catalog_Service.Features.Shared;

namespace Catalog_Service.Features.OffersFeature.DeleteOffer
{
    /// <summary>
    /// US-G04: Delete Offer
    /// </summary>
    public record DeleteOfferCommand(int OfferId) : IRequest<EndpointResponse<bool>>;
}

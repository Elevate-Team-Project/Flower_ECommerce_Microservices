using MediatR;
using Catalog_Service.Features.OffersFeature.CreateOffer;
using Catalog_Service.Features.Shared;

namespace Catalog_Service.Features.OffersFeature.GetOfferById
{
    public record GetOfferByIdQuery(int OfferId) : IRequest<EndpointResponse<OfferDto>>;
}

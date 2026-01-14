using MediatR;
using Promotion_Service.Features.Offers.CreateOffer;
using Promotion_Service.Features.Shared;

namespace Promotion_Service.Features.Offers.GetOfferById
{
    public record GetOfferByIdQuery(int OfferId) : IRequest<EndpointResponse<OfferDto>>;
}

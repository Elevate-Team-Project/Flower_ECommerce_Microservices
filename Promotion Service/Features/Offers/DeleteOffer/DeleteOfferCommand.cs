using MediatR;
using Promotion_Service.Features.Shared;

namespace Promotion_Service.Features.Offers.DeleteOffer
{
    /// <summary>
    /// US-G04: Delete Offer
    /// </summary>
    public record DeleteOfferCommand(int OfferId) : IRequest<EndpointResponse<bool>>;
}

using MediatR;
using Promotion_Service.Entities;
using Promotion_Service.Features.Shared;

namespace Promotion_Service.Features.Offers.CreateOffer
{
    /// <summary>
    /// US-G01: Create Offer
    /// </summary>
    public record CreateOfferCommand(
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

    public record OfferDto(
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
        DateTime StartDate,
        DateTime EndDate,
        OfferStatus Status,
        int Priority,
        bool IsActive,
        DateTime CreatedAt
    );
}

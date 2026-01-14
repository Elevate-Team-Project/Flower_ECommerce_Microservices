using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Promotion_Service.Features.Offers.UpdateOffer
{
    public static class Endpoints
    {
        public static void MapUpdateOfferEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPut("/api/offers/{id}", async (
                int id,
                [FromBody] UpdateOfferRequest request,
                IMediator mediator) =>
            {
                var command = new UpdateOfferCommand(
                    id,
                    request.Name,
                    request.NameAr,
                    request.Description,
                    request.DescriptionAr,
                    request.Type,
                    request.DiscountValue,
                    request.MaxDiscountAmount,
                    request.TargetType,
                    request.ProductId,
                    request.CategoryId,
                    request.OccasionId,
                    request.StartDate,
                    request.EndDate,
                    request.Priority,
                    request.AdminNotes
                );

                var result = await mediator.Send(command);
                return result.IsSuccess
                    ? Results.Ok(result)
                    : result.StatusCode == 404
                        ? Results.NotFound(result)
                        : Results.BadRequest(result);
            })
            .WithName("UpdateOffer")
            .WithTags("Offers")
            .WithSummary("Update an offer (US-G03)")
            .Produces<UpdateOfferCommand>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization("AdminPolicy");
        }
    }

    public record UpdateOfferRequest(
        string Name,
        string? NameAr,
        string? Description,
        string? DescriptionAr,
        Entities.OfferType Type,
        decimal DiscountValue,
        decimal? MaxDiscountAmount,
        Entities.OfferTargetType TargetType,
        int? ProductId,
        int? CategoryId,
        int? OccasionId,
        DateTime StartDate,
        DateTime EndDate,
        int Priority = 0,
        string? AdminNotes = null
    );
}

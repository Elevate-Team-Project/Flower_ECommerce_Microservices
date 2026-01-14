using MediatR;

namespace Promotion_Service.Features.Offers.DeleteOffer
{
    public static class Endpoints
    {
        public static void MapDeleteOfferEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/offers/{id}", async (int id, IMediator mediator) =>
            {
                var result = await mediator.Send(new DeleteOfferCommand(id));
                return result.IsSuccess
                    ? Results.Ok(result)
                    : Results.NotFound(result);
            })
            .WithName("DeleteOffer")
            .WithTags("Offers")
            .WithSummary("Delete an offer (US-G04)")
            .WithDescription("Soft deletes an offer. Affected product prices will revert to default values.")
            .Produces<DeleteOfferCommand>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization("AdminPolicy");
        }
    }
}

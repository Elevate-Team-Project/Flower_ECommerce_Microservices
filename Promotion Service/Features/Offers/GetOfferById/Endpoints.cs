using MediatR;

namespace Promotion_Service.Features.Offers.GetOfferById
{
    public static class Endpoints
    {
        public static void MapGetOfferByIdEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/offers/{id}", async (int id, IMediator mediator) =>
            {
                var result = await mediator.Send(new GetOfferByIdQuery(id));
                return result.IsSuccess
                    ? Results.Ok(result)
                    : Results.NotFound(result);
            })
            .WithName("GetOfferById")
            .WithTags("Offers")
            .WithSummary("Get offer by ID")
            .Produces<GetOfferByIdQuery>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .RequireAuthorization("AdminPolicy");
        }
    }
}

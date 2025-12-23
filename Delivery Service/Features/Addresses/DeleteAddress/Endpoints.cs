using MediatR;

namespace Delivery_Service.Features.Addresses.DeleteAddress
{
    public static class Endpoints
    {
        public static void MapDeleteAddressEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/addresses/{addressId}", async (int addressId, string userId, IMediator mediator) =>
            {
                var result = await mediator.Send(new DeleteAddressCommand(addressId, userId));
                return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
            })
            .WithName("DeleteAddress")
            .WithTags("Addresses")
            .RequireAuthorization();
        }
    }
}

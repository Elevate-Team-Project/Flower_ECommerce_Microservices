using MediatR;

namespace Ordering_Service.Features.Delivery.Addresses.SetDefaultAddress
{
    public static class Endpoints
    {
        public static void MapSetDefaultAddressEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPut("/api/addresses/{addressId}/default", async (int addressId, string userId, IMediator mediator) =>
            {
                var result = await mediator.Send(new SetDefaultAddressCommand(addressId, userId));
                return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
            })
            .WithName("SetDefaultAddress")
            .WithTags("Addresses")
            .RequireAuthorization();
        }
    }
}

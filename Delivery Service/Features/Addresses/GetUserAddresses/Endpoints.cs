using MediatR;

namespace Delivery_Service.Features.Addresses.GetUserAddresses
{
    public static class Endpoints
    {
        public static void MapGetUserAddressesEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/addresses/user/{userId}", async (string userId, IMediator mediator) =>
            {
                var result = await mediator.Send(new GetUserAddressesQuery(userId));
                return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
            })
            .WithName("GetUserAddresses")
            .WithTags("Addresses")
            .RequireAuthorization();
        }
    }
}

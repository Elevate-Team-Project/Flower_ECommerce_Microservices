using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Delivery_Service.Features.Addresses.CreateAddress
{
    public static class Endpoints
    {
        public static void MapCreateAddressEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/addresses", async ([FromBody] CreateAddressCommand command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return result.IsSuccess
                    ? Results.Created($"/api/addresses/{result.Data?.Id}", result)
                    : Results.BadRequest(result);
            })
            .WithName("CreateAddress")
            .WithTags("Addresses")
            .Produces<AddressDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);
            
        }
    }
}

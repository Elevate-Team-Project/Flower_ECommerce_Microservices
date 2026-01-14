using Delivery_Service.Features.Addresses.CreateAddress.Create_Address_automatically.DTOs;
using Delivery_Service.Features.Shared;
using MediatR;

namespace Delivery_Service.Features.Addresses.CreateAddress.Create_Address_automatically;

public static class Endpoint
{
    public static void MapCreateAddressAutomaticallyEndpoints(this WebApplication app)
    {
        app.MapPost("/addresses/auto",
            async (CreateAddressRequest request, IMediator mediator, CancellationToken ct) =>
            {
                var result = await mediator.Send(new CreateAddressAutomaticallyCommand(request), ct);

                if (!result.IsSuccess)
                    return Results.BadRequest(result);

                return Results.Ok(result);
            })
            .WithName("CreateAddressAutomatically")
            .WithTags("Addresses");
    }
}

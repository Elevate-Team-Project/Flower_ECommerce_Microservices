using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Delivery_Service.Features.Addresses.UpdateAddress
{
    public static class Endpoints
    {
        public static void MapUpdateAddressEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPut("/api/addresses/{addressId}", async (int addressId, [FromBody] UpdateAddressRequest request, IMediator mediator) =>
            {
                var command = new UpdateAddressCommand(
                    addressId, request.UserId, request.AddressLabel, request.FullName,
                    request.Phone, request.Street, request.City, request.State,
                    request.PostalCode, request.Country, request.Notes
                );
                var result = await mediator.Send(command);
                return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
            })
            .WithName("UpdateAddress")
            .WithTags("Addresses")
            .RequireAuthorization();
        }
    }

    public record UpdateAddressRequest(
        string UserId, string AddressLabel, string FullName, string Phone,
        string Street, string City, string State, string PostalCode, string Country, string? Notes
    );
}

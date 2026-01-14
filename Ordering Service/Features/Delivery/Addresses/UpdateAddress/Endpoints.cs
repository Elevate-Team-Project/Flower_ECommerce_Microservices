using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ordering_Service.Features.Delivery.Addresses.UpdateAddress
{
    public static class Endpoints
    {
        public static void MapUpdateAddressEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPut("/api/addresses/{addressId}", async (int addressId, [FromBody] UpdateAddressRequest request, IMediator mediator) =>
            {
                var command = new UpdateAddressCommand(
                    addressId,
                    request.UserId,
                    request.AddressLabel,
                    request.FullName,
                    request.Phone,
                    request.Latitude,
                    request.Longitude,
                    request.Governorate,
                    request.City,
                    request.Street,
                    request.Building,
                    request.Floor,
                    request.Apartment,
                    request.PostalCode,
                    request.Country,
                    request.Notes,
                    request.Landmark
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
        string UserId,
        string AddressLabel,
        string FullName,
        string Phone,
        double? Latitude,
        double? Longitude,
        string Governorate,
        string City,
        string Street,
        string? Building,
        string? Floor,
        string? Apartment,
        string? PostalCode,
        string Country,
        string? Notes,
        string? Landmark
    );
}

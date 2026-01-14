using MediatR;
using Microsoft.AspNetCore.Mvc;
using Catalog_Service.Features.Shared;

namespace Catalog_Service.Features.RegistrationCodesFeature.ApplyRegistrationCode
{
    public static class Endpoints
    {
        public static void MapApplyRegistrationCodeEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/registration-codes/apply", async (
                [FromBody] ApplyRegistrationCodeRequest request,
                HttpContext context,
                IMediator mediator) =>
            {
                // This endpoint should be authenticated
                var userId = context.User.Identity?.Name;
                if (string.IsNullOrEmpty(userId))
                    return Results.Unauthorized();

                var ipAddress = context.Connection.RemoteIpAddress?.ToString();

                var command = new ApplyRegistrationCodeCommand(request.Code, userId, ipAddress);
                var result = await mediator.Send(command);

                return result.IsSuccess
                    ? Results.Ok(result)
                    : Results.BadRequest(result);
            })
            .WithName("ApplyRegistrationCode")
            .WithTags("Registration Codes")
            .WithSummary("Apply a registration code to the current user")
            .Produces<EndpointResponse<ApplyRegistrationCodeResult>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization();
        }
    }

    public record ApplyRegistrationCodeRequest(string Code);
}

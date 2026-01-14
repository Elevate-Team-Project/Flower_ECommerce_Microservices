using MediatR;
using Microsoft.AspNetCore.Mvc;
using Promotion_Service.Features.Shared;

namespace Promotion_Service.Features.RegistrationCodes.ValidateRegistrationCode
{
    public static class Endpoints
    {
        public static void MapValidateRegistrationCodeEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/registration-codes/validate", async (
                [FromBody] ValidateRegistrationCodeRequest request,
                HttpContext context,
                IMediator mediator) =>
            {
                // Optionally get current user ID to check limits, though registration codes are often for new users
                var userId = context.User.Identity?.IsAuthenticated == true ? context.User.Identity.Name : null;

                var command = new ValidateRegistrationCodeCommand(request.Code, userId);
                var result = await mediator.Send(command);

                return Results.Ok(result);
            })
            .WithName("ValidateRegistrationCode")
            .WithTags("Registration Codes")
            .WithSummary("Validate a registration code")
            .Produces<EndpointResponse<RegistrationCodeValidationResult>>(StatusCodes.Status200OK);
        }
    }

    public record ValidateRegistrationCodeRequest(string Code);
}

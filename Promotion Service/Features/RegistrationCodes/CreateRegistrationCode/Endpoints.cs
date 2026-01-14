using MediatR;
using Promotion_Service.Features.Shared;
using Microsoft.AspNetCore.Mvc;
using Promotion_Service.Entities;

namespace Promotion_Service.Features.RegistrationCodes.CreateRegistrationCode
{
    public static class Endpoints
    {
        public static void MapCreateRegistrationCodeEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/registration-codes", async (
                [FromBody] CreateRegistrationCodeRequest request,
                IMediator mediator) =>
            {
                var command = new CreateRegistrationCodeCommand(
                    request.Code,
                    request.Description,
                    request.Type,
                    request.TargetCustomerGroupId,
                    request.WelcomeCreditAmount,
                    request.Currency,
                    request.MaxTotalUsage,
                    request.MaxUsagePerUser,
                    request.ValidFrom,
                    request.ValidUntil,
                    request.AdminNotes,
                    request.IsActive
                );

                var result = await mediator.Send(command);

                return result.IsSuccess
                    ? Results.Created($"/api/registration-codes/{result.Data?.Id}", result)
                    : Results.BadRequest(result);
            })
            .WithName("CreateRegistrationCode")
            .WithTags("Registration Codes")
            .WithSummary("Create a new registration code (Admin)")
            .Produces<EndpointResponse<RegistrationCodeDto>>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest)
            .RequireAuthorization("AdminPolicy");
        }
    }

    public record CreateRegistrationCodeRequest(
        string Code,
        string? Description,
        RegistrationCodeType Type,
        int? TargetCustomerGroupId,
        decimal? WelcomeCreditAmount,
        string Currency,
        int? MaxTotalUsage,
        int? MaxUsagePerUser,
        DateTime ValidFrom,
        DateTime ValidUntil,
        string? AdminNotes,
        bool IsActive = true
    );
}

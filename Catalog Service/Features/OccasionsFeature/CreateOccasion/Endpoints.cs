using Catalog_Service.Features.Shared;
using MediatR;

namespace Catalog_Service.Features.OccasionsFeature.CreateOccasion
{
    public static class Endpoints
    {
        public static void MapCreateOccasionEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/v1/occasions", async (
                CreateOccasionDto dto,
                IMediator mediator) =>
            {
                var result = await mediator.Send(new CreateOccasionCommand(dto));

                if (!result.IsSuccess)
                {
                    return EndpointResponse<RequestResponse<CreateOccasionDto>>.ErrorResponse(
                        result.Message,
                        400
                    );
                }

                return EndpointResponse<RequestResponse<CreateOccasionDto>>.SuccessResponse(
                    result,
                    "Occasion created successfully",
                    201
                );
            })
            //.RequireAuthorization("Admin")
            .WithSummary("Create a new occasion")
            .WithDescription("Creates a new occasion with unique name and active status.");
        }
    }
}

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
                var command = new CreateOccasionCommand(dto);
                var result = await mediator.Send(command);
                return Results.Created($"/api/occasions/{result.Name}", result);
            })
            //.RequireAuthorization("Admin")
            .WithSummary("Create a new occasion")
            .WithDescription("Creates a new occasion with unique name and active status.");
        }
    }
}

using MediatR;

namespace Catalog_Service.Features.CategoriesFeature.UpdateCategory
{
    public static class Endpoints
    {
        public static void MapUpdateCategoryEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPut("/api/v1/categories/{id}", async (
                int id,
                UpdateCategoryDto dto,
                IMediator mediator) =>
            {
                var result = await mediator.Send(new UpdateCategoryCommand(id, dto));
                return Results.Ok(result);
            })
                //.RequireAuthorization("Admin")
            .WithName("UpdateCategory")
            .WithSummary("Update an existing category")
            .WithDescription("Allows admin to update name, image, and parent category.");
        }
    }
}

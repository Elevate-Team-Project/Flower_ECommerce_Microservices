using MediatR;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Catalog_Service.Features.CategoriesFeature.CreateCategory
{
    public static class Endpoints
    {

        public static void MapCategoryEndpoints(this WebApplication app)
        {
            app.MapPost("/api/categories", async (
                CreateCategoryCommand command,
                IMediator mediator) =>
            {
                var id = await mediator.Send(command);

                return Results.Created($"/api/categories/{id}", new { Id = id });
            });
        }
    }

}

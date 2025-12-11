using Catalog_Service.Features.Shared;
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

                if (!result.IsSuccess)
                {
                    return EndpointResponse<RequestResponse<bool>>.ErrorResponse(
                        result.Message,
                        statusCode: 400
                    );
                }

                return EndpointResponse<RequestResponse<bool>>.SuccessResponse(
                    result,
                    "Category updated successfully",
                    statusCode: 200
                );
            })
            //.RequireAuthorization("Admin")
            .WithName("UpdateCategory")
            .WithSummary("Update an existing category")
            .WithDescription("Allows admin to update name, image, and parent category.");
        }
    }
}

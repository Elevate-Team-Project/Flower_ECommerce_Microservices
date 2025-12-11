using Catalog_Service.Features.Shared;
using MediatR;

namespace Catalog_Service.Features.CategoriesFeature.UpdateCategoryStatus
{
    public static class Endpoints
    {
        public static void MapCategoryStatusEndpoints(this IEndpointRouteBuilder app)
        {
            #region Activate 
            app.MapPatch("/api/v1/categories/{id}/activate", async (
                int id,
                IMediator mediator) =>
            {
                var result = await mediator.Send(new ActivateCategoryCommand(id));

                if (!result.IsSuccess)
                    return EndpointResponse<RequestResponse<bool>>.ErrorResponse(result.Message, 400);

                return EndpointResponse<RequestResponse<bool>>.SuccessResponse(
                    result,
                    "Category activated successfully",
                    200
                );
            })
            //.RequireAuthorization("Admin")
            .WithSummary("Activate a category")
            .WithDescription("Marks a category as active.");
            #endregion

            #region Deactivate

            app.MapPatch("/api/v1/categories/{id}/deactivate", async (
                int id,
                IMediator mediator) =>
            {
                var result = await mediator.Send(new DeactivateCategoryCommand(id));

                if (!result.IsSuccess)
                    return EndpointResponse<RequestResponse<bool>>.ErrorResponse(result.Message, 400);

                return EndpointResponse<RequestResponse<bool>>.SuccessResponse(
                    result,
                    "Category deactivated successfully",
                    200
                );
            })
            //.RequireAuthorization("Admin")
            .WithSummary("Deactivate a category")
            .WithDescription("Marks a category as inactive.");
            #endregion
        }
    }
}

using Catalog_Service.Features.Shared;
using MediatR;

namespace Catalog_Service.Features.RateDeliveredItem
{
    public static class Endpoints
    {
        public static void MapAddReviewEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapPost("/api/v1/products/{productId}/reviews", async (
                int productId,
                AddReviewDto dto,
                HttpContext context,
                IMediator mediator) =>
            {
                
                var userId = context.User.Identity?.Name ?? "test-user";
                var userName = "Test User";

                var result = await mediator.Send(
                    new AddReviewCommand(productId, userId, userName, dto.Rating, dto.Comment)
                );

                if (!result.IsSuccess)
                {
                    return Results.Json(
                        EndpointResponse<RequestResponse<bool>>
                            .ErrorResponse(result.Message, 400),
                        statusCode: 400
                    );
                }

                return Results.Json(
                    EndpointResponse<RequestResponse<bool>>
                        .SuccessResponse(result, "Review added successfully"),
                    statusCode: 200
                );
            })
            .WithSummary("Add product review")
            .WithDescription("Allows user to add rating and feedback for delivered products.");
        }
    }
}

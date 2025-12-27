using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ordering_Service.Features.Shared;

namespace Ordering_Service.Features.Orders
{
    public static class Endpoints
    {
        public static void MapGetOrdersStatusEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/orders/{orderId}/status", async (
            int orderId,
            IMediator mediator,
            int page = 1,
            int pageSize = 10,
            string? status = null) =>
            {
                var query = new GetOrdersStatusQuery(orderId, page, pageSize, status);
                var result = await mediator.Send(query);

                return result.IsSuccess
                    ? Results.Ok(result)
                    : Results.BadRequest(result);
            })
            .WithName("GetOrdersStatus")
            .WithTags("Orders");
        }
    }
    }

using MediatR;
using Notification_Service.Features.Shared;

namespace Notification_Service.Features.Notifications.GetNotifications
{
    public static class Endpoints
    {
        public static void MapGetNotificationsEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/notifications/{userId}", async (
                string userId,
                string? type,
                bool? isRead,
                int pageNumber,
                int pageSize,
                IMediator mediator) =>
            {
                var query = new GetNotificationsQuery(
                    userId, 
                    type, 
                    isRead, 
                    pageNumber > 0 ? pageNumber : 1, 
                    pageSize > 0 ? pageSize : 20);
                    
                var result = await mediator.Send(query);
                return Results.Ok(result);
            })
            .WithName("GetNotifications")
            .WithTags("Notifications")
            .Produces<EndpointResponse<GetNotificationsResult>>(200);
        }
    }
}

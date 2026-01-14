using MediatR;
using Notification_Service.Features.Shared;

namespace Notification_Service.Features.Notifications.MarkRead
{
    public static class Endpoints
    {
        public static void MapMarkReadEndpoints(this IEndpointRouteBuilder app)
        {
            // Mark specific notification as read
            app.MapPost("/api/notifications/{id:int}/read", async (
                int id,
                string userId,
                IMediator mediator) =>
            {
                var command = new MarkNotificationsReadCommand(userId, new List<int> { id });
                var result = await mediator.Send(command);
                return Results.Ok(result);
            })
            .WithName("MarkNotificationRead")
            .WithTags("Notifications")
            .Produces<EndpointResponse<MarkReadResult>>(200);

            // Mark all notifications as read
            app.MapPost("/api/notifications/{userId}/read-all", async (
                string userId,
                IMediator mediator) =>
            {
                var command = new MarkNotificationsReadCommand(userId, MarkAll: true);
                var result = await mediator.Send(command);
                return Results.Ok(result);
            })
            .WithName("MarkAllNotificationsRead")
            .WithTags("Notifications")
            .Produces<EndpointResponse<MarkReadResult>>(200);
        }
    }
}

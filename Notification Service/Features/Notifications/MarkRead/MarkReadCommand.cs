using MediatR;
using Notification_Service.Features.Shared;

namespace Notification_Service.Features.Notifications.MarkRead
{
    /// <summary>
    /// US-F07: Mark notification(s) as read
    /// </summary>
    public record MarkNotificationsReadCommand(
        string UserId,
        List<int>? NotificationIds = null,  // If null, marks all as read
        bool MarkAll = false
    ) : IRequest<EndpointResponse<MarkReadResult>>;

    public record MarkReadResult(int UpdatedCount);
}

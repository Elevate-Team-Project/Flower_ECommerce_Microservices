using MediatR;
using Notification_Service.Features.Shared;

namespace Notification_Service.Features.Notifications.GetNotifications
{
    /// <summary>
    /// US-F07: View Notifications
    /// Gets all notifications for a user
    /// </summary>
    public record GetNotificationsQuery(
        string UserId,
        string? Type = null,
        bool? IsRead = null,
        int PageNumber = 1,
        int PageSize = 20
    ) : IRequest<EndpointResponse<GetNotificationsResult>>;

    public record GetNotificationsResult(
        List<NotificationDto> Notifications,
        int TotalCount,
        int UnreadCount
    );

    public record NotificationDto(
        int Id,
        string Title,
        string Description,
        string Type,
        bool IsRead,
        DateTime CreatedAt,
        DateTime? ReadAt,
        string? ReferenceId,
        string? ActionUrl
    );
}

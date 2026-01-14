using MediatR;
using Microsoft.EntityFrameworkCore;
using Notification_Service.Features.Shared;
using Notification_Service.Infrastructure;

namespace Notification_Service.Features.Notifications.MarkRead
{
    public class MarkNotificationsReadHandler : IRequestHandler<MarkNotificationsReadCommand, EndpointResponse<MarkReadResult>>
    {
        private readonly NotificationDbContext _context;

        public MarkNotificationsReadHandler(NotificationDbContext context)
        {
            _context = context;
        }

        public async Task<EndpointResponse<MarkReadResult>> Handle(
            MarkNotificationsReadCommand request,
            CancellationToken cancellationToken)
        {
            var now = DateTime.UtcNow;
            int updatedCount;

            if (request.MarkAll || request.NotificationIds == null || !request.NotificationIds.Any())
            {
                // Mark all unread notifications as read
                updatedCount = await _context.Notifications
                    .Where(n => n.UserId == request.UserId && !n.IsRead)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(n => n.IsRead, true)
                        .SetProperty(n => n.ReadAt, now),
                        cancellationToken);
            }
            else
            {
                // Mark specific notifications as read
                updatedCount = await _context.Notifications
                    .Where(n => n.UserId == request.UserId && 
                                request.NotificationIds.Contains(n.Id) && 
                                !n.IsRead)
                    .ExecuteUpdateAsync(s => s
                        .SetProperty(n => n.IsRead, true)
                        .SetProperty(n => n.ReadAt, now),
                        cancellationToken);
            }

            return EndpointResponse<MarkReadResult>.SuccessResponse(
                new MarkReadResult(updatedCount),
                $"Marked {updatedCount} notification(s) as read");
        }
    }
}

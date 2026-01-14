using MediatR;
using Microsoft.EntityFrameworkCore;
using Notification_Service.Features.Shared;
using Notification_Service.Infrastructure;

namespace Notification_Service.Features.Notifications.GetNotifications
{
    public class GetNotificationsHandler : IRequestHandler<GetNotificationsQuery, EndpointResponse<GetNotificationsResult>>
    {
        private readonly NotificationDbContext _context;

        public GetNotificationsHandler(NotificationDbContext context)
        {
            _context = context;
        }

        public async Task<EndpointResponse<GetNotificationsResult>> Handle(
            GetNotificationsQuery request, 
            CancellationToken cancellationToken)
        {
            var query = _context.Notifications
                .Where(n => n.UserId == request.UserId);

            // Filter by type
            if (!string.IsNullOrEmpty(request.Type))
            {
                query = query.Where(n => n.Type == request.Type);
            }

            // Filter by read status
            if (request.IsRead.HasValue)
            {
                query = query.Where(n => n.IsRead == request.IsRead.Value);
            }

            var totalCount = await query.CountAsync(cancellationToken);
            var unreadCount = await query.Where(n => !n.IsRead).CountAsync(cancellationToken);

            var notifications = await query
                .OrderByDescending(n => n.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .Select(n => new NotificationDto(
                    n.Id,
                    n.Title,
                    n.Description,
                    n.Type,
                    n.IsRead,
                    n.CreatedAt,
                    n.ReadAt,
                    n.ReferenceId,
                    n.ActionUrl
                ))
                .ToListAsync(cancellationToken);

            var result = new GetNotificationsResult(notifications, totalCount, unreadCount);
            return EndpointResponse<GetNotificationsResult>.SuccessResponse(result);
        }
    }
}

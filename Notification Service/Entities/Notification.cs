using BuildingBlocks.SharedEntities;

namespace Notification_Service.Entities
{
    /// <summary>
    /// Represents a user notification.
    /// US-F07: View Notifications
    /// </summary>
    public class Notification : BaseEntity
    {
        public string UserId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        
        /// <summary>
        /// Notification type: Offer, Reminder, Order, System
        /// </summary>
        public string Type { get; set; } = "System";
        
        public bool IsRead { get; set; } = false;
        public DateTime? ReadAt { get; set; }
        
        /// <summary>
        /// Optional reference ID (e.g., OrderId, OfferId)
        /// </summary>
        public string? ReferenceId { get; set; }
        
        /// <summary>
        /// Optional deep link for the notification action
        /// </summary>
        public string? ActionUrl { get; set; }
    }
}

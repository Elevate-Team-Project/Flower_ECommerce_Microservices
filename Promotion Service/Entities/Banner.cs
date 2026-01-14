using BuildingBlocks.SharedEntities;

namespace Promotion_Service.Entities
{
    /// <summary>
    /// Promotional banners for homepage, category pages, etc.
    /// </summary>
    public class Banner : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string? TitleAr { get; set; }
        public string? Subtitle { get; set; }
        public string? SubtitleAr { get; set; }

        // Images
        public string DesktopImageUrl { get; set; } = string.Empty;
        public string? MobileImageUrl { get; set; }

        // Call to Action
        public string? CtaText { get; set; }
        public string? CtaTextAr { get; set; }
        public string? CtaLink { get; set; }

        // Positioning
        public BannerPosition Position { get; set; } = BannerPosition.Homepage;
        public int SortOrder { get; set; } = 0;

        // Scheduling
        public DateTime ValidFrom { get; set; }
        public DateTime ValidUntil { get; set; }

        // Computed
        public bool IsExpired => DateTime.UtcNow > ValidUntil;
        public bool IsScheduled => DateTime.UtcNow < ValidFrom;
        public bool IsCurrentlyActive => !IsExpired && !IsScheduled && IsActive;
    }

    public enum BannerPosition
    {
        Homepage,
        CategoryPage,
        ProductPage,
        Checkout,
        Cart
    }
}

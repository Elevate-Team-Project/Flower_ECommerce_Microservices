namespace Promotion_Service.Features.Loyalty
{
    public class LoyaltySettings
    {
        public decimal BaseEarningRate { get; set; } = 0.1m; // Points per currency unit (e.g., 0.1 = 1 point per 10 currency)
        public decimal RedemptionRate { get; set; } = 0.1m; // Currency value per point (e.g., 0.1 = 1 Point is 0.1 currency)
        public decimal MaxRedemptionPercentage { get; set; } = 0.5m; // Max % of order total payable with points
    }
}

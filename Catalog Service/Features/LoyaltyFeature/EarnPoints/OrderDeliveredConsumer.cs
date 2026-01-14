using BuildingBlocks.IntegrationEvents;
using BuildingBlocks.Interfaces;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Catalog_Service.Entities;
using Catalog_Service.Features.Shared;

namespace Catalog_Service.Features.LoyaltyFeature.EarnPoints
{
    public class OrderDeliveredConsumer : IConsumer<OrderDeliveredEvent>
    {
        private readonly IBaseRepository<LoyaltyAccount> _accountRepository;
        private readonly IBaseRepository<LoyaltyTransaction> _transactionRepository;
        private readonly IBaseRepository<LoyaltyTier> _tierRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly LoyaltySettings _settings;
        private readonly ILogger<OrderDeliveredConsumer> _logger;

        public OrderDeliveredConsumer(
            IBaseRepository<LoyaltyAccount> accountRepository,
            IBaseRepository<LoyaltyTransaction> transactionRepository,
            IBaseRepository<LoyaltyTier> tierRepository,
            IUnitOfWork unitOfWork,
            IOptions<LoyaltySettings> settings,
            ILogger<OrderDeliveredConsumer> logger)
        {
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _tierRepository = tierRepository;
            _unitOfWork = unitOfWork;
            _settings = settings.Value;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<OrderDeliveredEvent> context)
        {
            var message = context.Message;
            _logger.LogInformation("Processing loyalty points for order {OrderId} (User: {UserId})", message.OrderId, message.UserId);

            // Get or create loyalty account with Tier info
            var account = await _accountRepository.Get(a => a.UserId == message.UserId)
                .Include(a => a.Tier)
                .FirstOrDefaultAsync();

            if (account == null)
            {
                account = new LoyaltyAccount
                {
                    UserId = message.UserId,
                    CurrentPoints = 0,
                    TotalEarnedPoints = 0,
                    TierId = 1 // Default Tier (Silver/Basic)
                };
                await _accountRepository.AddAsync(account);
                await _unitOfWork.SaveChangesAsync(); // Save to get ID and allow including Tier later if needed, though for default we assume ID 1 exists or is lowest
                
                // Reload to ensure Tier navigation is populated if possible, or just proceed with default assumptions
                // For simplicity, we assume TierId 1 has multiplier 1.0 if not loaded yet.
            }

            // Determine Multiplier
            decimal multiplier = account.Tier?.PointsMultiplier ?? 1.0m;

            // Calculate points
            int pointsEarned = (int)(message.OrderTotal * _settings.BaseEarningRate * multiplier);

            if (pointsEarned <= 0)
                return;

            // Create Transaction
            var transaction = new LoyaltyTransaction
            {
                LoyaltyAccountId = account.Id,
                Type = TransactionType.Earned, // Fixed Enum
                Points = pointsEarned,
                Description = $"Points earned from Order #{message.OrderId}",
                OrderId = message.OrderId,
                OrderAmount = message.OrderTotal,
                BalanceAfter = account.CurrentPoints + pointsEarned
            };

            await _transactionRepository.AddAsync(transaction);

            // Update Account Balance
            account.CurrentPoints += pointsEarned;
            account.TotalEarnedPoints += pointsEarned;

            // Check for Tier Upgrade
            await CheckForTierUpgrade(account);

            _accountRepository.Update(account);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Awarded {Points} points to user {UserId}. New Balance: {Balance}. Tier: {TierId}", 
                pointsEarned, message.UserId, account.CurrentPoints, account.TierId);
        }

        private async Task CheckForTierUpgrade(LoyaltyAccount account)
        {
            // Get all tiers ordered by requirements
            var tiers = await _tierRepository.GetAll()
                .OrderBy(t => t.MinPoints)
                .ToListAsync();

            // Find highest achievable tier based on TotalEarnedPoints
            var eligibleTier = tiers.LastOrDefault(t => account.TotalEarnedPoints >= t.MinPoints);

            if (eligibleTier != null && eligibleTier.Id != account.TierId)
            {
                // Verify we are upgrading (not downgrading, though total points usually only go up)
                // Assuming higher MinPoints means higher Tier.
                if (eligibleTier.MinPoints > (account.Tier?.MinPoints ?? 0))
                {
                    var oldTierId = account.TierId;
                    account.TierId = eligibleTier.Id;
                    _logger.LogInformation("User {UserId} upgraded from Tier {OldTier} to {NewTier}", account.UserId, oldTierId, eligibleTier.Id);
                    
                    // Optional: Add a notification or bonus transaction here
                    account.Tier = eligibleTier; // update navigation property for current context
                }
            }
        }
    }
}

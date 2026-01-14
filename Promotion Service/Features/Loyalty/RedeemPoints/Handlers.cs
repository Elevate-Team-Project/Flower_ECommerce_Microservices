using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Promotion_Service.Entities;
using Promotion_Service.Features.Shared;

namespace Promotion_Service.Features.Loyalty.RedeemPoints
{
    public class RedeemPointsHandler : IRequestHandler<RedeemPointsCommand, EndpointResponse<RedemptionResult>>
    {
        private readonly IBaseRepository<LoyaltyAccount> _accountRepository;
        private readonly IBaseRepository<LoyaltyTransaction> _transactionRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<RedeemPointsHandler> _logger;

        // Points to currency conversion rate (e.g., 10 points = 1 EGP)
        private const decimal PointsToCurrencyRate = 0.1m;
        // Maximum percentage of order that can be paid with points
        private const decimal MaxRedemptionPercentage = 0.5m;

        public RedeemPointsHandler(
            IBaseRepository<LoyaltyAccount> accountRepository,
            IBaseRepository<LoyaltyTransaction> transactionRepository,
            IUnitOfWork unitOfWork,
            ILogger<RedeemPointsHandler> logger)
        {
            _accountRepository = accountRepository;
            _transactionRepository = transactionRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<EndpointResponse<RedemptionResult>> Handle(
            RedeemPointsCommand request,
            CancellationToken cancellationToken)
        {
            var account = await _accountRepository.GetAll()
                .FirstOrDefaultAsync(a => a.UserId == request.UserId, cancellationToken);

            if (account == null)
                return EndpointResponse<RedemptionResult>.NotFoundResponse("Loyalty account not found");

            if (request.Points <= 0)
                return EndpointResponse<RedemptionResult>.ErrorResponse("Points to redeem must be greater than 0");

            if (account.CurrentPoints < request.Points)
                return EndpointResponse<RedemptionResult>.ErrorResponse(
                    $"Insufficient points. Available: {account.CurrentPoints}");

            // Calculate max redeemable based on order amount
            if (request.OrderAmount.HasValue)
            {
                var maxDiscount = request.OrderAmount.Value * MaxRedemptionPercentage;
                var maxPoints = (int)(maxDiscount / PointsToCurrencyRate);

                if (request.Points > maxPoints)
                    return EndpointResponse<RedemptionResult>.ErrorResponse(
                        $"Maximum {maxPoints} points can be redeemed for this order (50% limit)");
            }

            // Calculate discount value
            var discountValue = request.Points * PointsToCurrencyRate;

            // Update account
            account.CurrentPoints -= request.Points;
            account.TotalRedeemedPoints += request.Points;
            _accountRepository.Update(account);

            // Create transaction record
            var transaction = new LoyaltyTransaction
            {
                LoyaltyAccountId = account.Id,
                Type = TransactionType.Redeemed,
                Points = request.Points,
                Description = $"Redeemed {request.Points} points for order",
                OrderId = request.OrderId,
                OrderAmount = request.OrderAmount,
                BalanceAfter = account.CurrentPoints
            };

            await _transactionRepository.AddAsync(transaction);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation(
                "User {UserId} redeemed {Points} points for {Discount:C}",
                request.UserId, request.Points, discountValue);

            var result = new RedemptionResult(
                transaction.Id,
                request.Points,
                discountValue,
                account.CurrentPoints
            );

            return EndpointResponse<RedemptionResult>.SuccessResponse(
                result, "Points redeemed successfully");
        }
    }
}

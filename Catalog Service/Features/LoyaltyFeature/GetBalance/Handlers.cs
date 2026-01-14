using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Catalog_Service.Entities;
using Catalog_Service.Features.Shared;

namespace Catalog_Service.Features.LoyaltyFeature.GetBalance
{
    public class GetLoyaltyBalanceHandler : IRequestHandler<GetLoyaltyBalanceQuery, EndpointResponse<LoyaltyBalanceDto>>
    {
        private readonly IBaseRepository<LoyaltyAccount> _accountRepository;
        private readonly IBaseRepository<LoyaltyTier> _tierRepository;
        private readonly IUnitOfWork _unitOfWork;

        public GetLoyaltyBalanceHandler(
            IBaseRepository<LoyaltyAccount> accountRepository,
            IBaseRepository<LoyaltyTier> tierRepository,
            IUnitOfWork unitOfWork)
        {
            _accountRepository = accountRepository;
            _tierRepository = tierRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<LoyaltyBalanceDto>> Handle(
            GetLoyaltyBalanceQuery request,
            CancellationToken cancellationToken)
        {
            var account = await _accountRepository.GetAll()
                .Include(a => a.Tier)
                .FirstOrDefaultAsync(a => a.UserId == request.UserId, cancellationToken);

            // Create account if doesn't exist
            if (account == null)
            {
                var defaultTier = await _tierRepository.GetAll()
                    .OrderBy(t => t.MinPoints)
                    .FirstOrDefaultAsync(cancellationToken);

                account = new LoyaltyAccount
                {
                    UserId = request.UserId,
                    TierId = defaultTier?.Id ?? 1,
                    Tier = defaultTier!
                };

                await _accountRepository.AddAsync(account);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }

            // Calculate points to next tier
            var nextTier = await _tierRepository.GetAll()
                .Where(t => t.MinPoints > account.TotalEarnedPoints)
                .OrderBy(t => t.MinPoints)
                .FirstOrDefaultAsync(cancellationToken);

            int pointsToNextTier = nextTier != null
                ? nextTier.MinPoints - account.TotalEarnedPoints
                : 0;

            var dto = new LoyaltyBalanceDto(
                account.Id,
                account.UserId,
                account.CurrentPoints,
                account.TotalEarnedPoints,
                account.TotalRedeemedPoints,
                account.Tier?.Name ?? "Bronze",
                account.TierId,
                account.Tier?.PointsMultiplier ?? 1.0m,
                account.Tier?.DiscountPercentage,
                account.Tier?.FreeShipping ?? false,
                pointsToNextTier
            );

            return EndpointResponse<LoyaltyBalanceDto>.SuccessResponse(
                dto, "Loyalty balance retrieved successfully");
        }
    }
}

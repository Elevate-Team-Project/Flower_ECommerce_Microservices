using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Promotion_Service.Entities;
using Promotion_Service.Features.Shared;

namespace Promotion_Service.Features.RegistrationCodes.ApplyRegistrationCode
{
    public class ApplyRegistrationCodeHandler : IRequestHandler<ApplyRegistrationCodeCommand, EndpointResponse<ApplyRegistrationCodeResult>>
    {
        private readonly IBaseRepository<RegistrationCode> _codeRepository;
        private readonly IBaseRepository<RegistrationCodeUsage> _usageRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ApplyRegistrationCodeHandler(
            IBaseRepository<RegistrationCode> codeRepository,
            IBaseRepository<RegistrationCodeUsage> usageRepository,
            IUnitOfWork unitOfWork)
        {
            _codeRepository = codeRepository;
            _usageRepository = usageRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<ApplyRegistrationCodeResult>> Handle(
            ApplyRegistrationCodeCommand request,
            CancellationToken cancellationToken)
        {
            var code = await _codeRepository.GetAll()
                .Include(c => c.Usages)
                .FirstOrDefaultAsync(c => c.Code == request.Code, cancellationToken);

            if (code == null)
                return EndpointResponse<ApplyRegistrationCodeResult>.NotFoundResponse("Code not found");

            if (!code.IsActive || DateTime.UtcNow < code.ValidFrom || DateTime.UtcNow > code.ValidUntil)
                return EndpointResponse<ApplyRegistrationCodeResult>.ErrorResponse("Code is not valid");

            if (code.HasReachedMaxUsage)
                return EndpointResponse<ApplyRegistrationCodeResult>.ErrorResponse("Code usage limit reached");

            // Check per-user usage
            if (code.MaxUsagePerUser.HasValue)
            {
                var userUsage = code.Usages.Count(u => u.UserId == request.UserId);
                if (userUsage >= code.MaxUsagePerUser.Value)
                    return EndpointResponse<ApplyRegistrationCodeResult>.ErrorResponse("You have already used this code");
            }

            // Record Usage
            var usage = new RegistrationCodeUsage
            {
                RegistrationCodeId = code.Id,
                UserId = request.UserId,
                IpAddress = request.IpAddress ?? "0.0.0.0",
                UsedAt = DateTime.UtcNow,
                GroupUpgradeApplied = code.Type == RegistrationCodeType.GroupUpgrade || code.Type == RegistrationCodeType.Both,
                WalletCreditApplied = code.Type == RegistrationCodeType.WalletCredit || code.Type == RegistrationCodeType.Both,
                CreditAmount = code.WelcomeCreditAmount
            };

            await _usageRepository.AddAsync(usage);

            // Update Code Counters
            code.CurrentUsageCount++;
            _codeRepository.Update(code);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            // TODO: Publish event for Auth/Payment service to actually apply the upgrade/credit
            // For now, we return the data so the caller (API Gateway or Frontend) can know what happened

            return EndpointResponse<ApplyRegistrationCodeResult>.SuccessResponse(
                new ApplyRegistrationCodeResult(
                    true,
                    "Code applied successfully",
                    code.Type,
                    code.WelcomeCreditAmount,
                    code.Currency,
                    code.TargetCustomerGroupId
                ),
                "Code applied successfully"
            );
        }
    }
}

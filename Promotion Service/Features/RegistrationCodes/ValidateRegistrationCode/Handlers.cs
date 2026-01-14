using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Promotion_Service.Entities;
using Promotion_Service.Features.Shared;

namespace Promotion_Service.Features.RegistrationCodes.ValidateRegistrationCode
{
    public class ValidateRegistrationCodeHandler : IRequestHandler<ValidateRegistrationCodeCommand, EndpointResponse<RegistrationCodeValidationResult>>
    {
        private readonly IBaseRepository<RegistrationCode> _codeRepository;

        public ValidateRegistrationCodeHandler(IBaseRepository<RegistrationCode> codeRepository)
        {
            _codeRepository = codeRepository;
        }

        public async Task<EndpointResponse<RegistrationCodeValidationResult>> Handle(
            ValidateRegistrationCodeCommand request,
            CancellationToken cancellationToken)
        {
            var code = await _codeRepository.GetAll()
                .Include(c => c.Usages)
                .FirstOrDefaultAsync(c => c.Code == request.Code, cancellationToken);

            if (code == null)
            {
                return EndpointResponse<RegistrationCodeValidationResult>.SuccessResponse(
                    new RegistrationCodeValidationResult(request.Code, false, "Code not found", RegistrationCodeType.Both, null, null, null));
            }

            if (!code.IsActive)
            {
                return EndpointResponse<RegistrationCodeValidationResult>.SuccessResponse(
                    new RegistrationCodeValidationResult(request.Code, false, "Code is not active", code.Type, null, null, null));
            }

            if (DateTime.UtcNow < code.ValidFrom || DateTime.UtcNow > code.ValidUntil)
            {
                return EndpointResponse<RegistrationCodeValidationResult>.SuccessResponse(
                    new RegistrationCodeValidationResult(request.Code, false, "Code is expired or not yet valid", code.Type, null, null, null));
            }

            if (code.HasReachedMaxUsage)
            {
                return EndpointResponse<RegistrationCodeValidationResult>.SuccessResponse(
                    new RegistrationCodeValidationResult(request.Code, false, "Code usage limit reached", code.Type, null, null, null));
            }

            // Check per-user limit if UserId provided
            if (!string.IsNullOrEmpty(request.UserId) && code.MaxUsagePerUser.HasValue)
            {
                var userUsage = code.Usages.Count(u => u.UserId == request.UserId);
                if (userUsage >= code.MaxUsagePerUser.Value)
                {
                    return EndpointResponse<RegistrationCodeValidationResult>.SuccessResponse(
                        new RegistrationCodeValidationResult(request.Code, false, "You have already used this code", code.Type, null, null, null));
                }
            }

            return EndpointResponse<RegistrationCodeValidationResult>.SuccessResponse(
                new RegistrationCodeValidationResult(
                    code.Code,
                    true,
                    "Code is valid",
                    code.Type,
                    code.WelcomeCreditAmount,
                    code.Currency,
                    code.TargetCustomerGroupId
                ),
                "Code is valid"
            );
        }
    }
}
